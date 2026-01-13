using UnityEngine;

[RequireComponent(typeof(Health))]
public class EnemyCombatController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Health health;
    [SerializeField] private EnemyMotor2D motor;
    [SerializeField] private EnemyIdleController idle;

    [Header("Player")]
    [SerializeField] private Transform player;

    [Header("Ranges")]
    [SerializeField] private float aggroRange = 8f;
    [SerializeField] private float attackRange = 1.2f;

    [Header("Movement")]
    [SerializeField] private float chaseSpeed = 2.5f;

    [Header("Attack")]
    [SerializeField] private float attackCooldown = 1.0f;
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private float attackRadius = 0.5f;
    [SerializeField] private LayerMask playerMask;

    [Header("Attack Safety")]
    [Tooltip("Only used if EndAttack animation event fails. Set longer than your longest attack clip.")]
    [SerializeField] private float attackFailsafeTime = 2.0f;

    [Header("Disengage")]
    [SerializeField] private float disengageTime = 1.0f;
    [SerializeField] private float yChaseTolerance = 0.9f;

    [Header("Post-attack behavior")]
    [SerializeField] private float postAttackStandTime = 0.25f; // stand briefly after attack
    [SerializeField] private float chaseWhenPlayerSpeedAbove = 0.05f; // ignore tiny jitter
    [SerializeField] private bool chaseOnlyIfSeparating = true;

    private float _postAttackUntil;
    private Vector3 _lastPlayerPos;

    // Animator param names (must match your controller)
    private const string PARAM_WALK = "AnimIsWalking";
    private const string TRIG_ATTACK = "AnimAttack";

    private Animator animator;
    private Transform attackPoint;

    private float _nextAttackTime;
    private bool _isAttacking;
    private float _attackFailsafeUntil;
    public bool IsAttacking => _isAttacking;

    private float _stunnedUntil;

    private float _disengageUntil;
    private bool IsDisengaged => Time.time < _disengageUntil;

    private void Awake()
    {
        if (health == null) health = GetComponent<Health>();
        if (motor == null) motor = GetComponent<EnemyMotor2D>();
        if (idle == null) idle = GetComponent<EnemyIdleController>();
    }

    private void Start()
    {
        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
            _lastPlayerPos = player.position;
        }
    }

    private void OnEnable()
    {
        if (health != null) health.OnDeath += HandleDeath;
    }

    private void OnDisable()
    {
        if (health != null) health.OnDeath -= HandleDeath;
    }

    public void ApplyVariantAnimatorAndAttackPoint(Animator a, Transform ap)
    {
        animator = a;
        attackPoint = ap;
        if (idle != null) idle.SetAnimator(a);
    }

    private void HandleDeath()
    {
        StopAttackLocks();
        motor.MoveHorizontally(0f, respectBlocks: false);
        enabled = false;
    }

    private void Update()
    {
        if (animator == null || motor == null || idle == null || player == null)
        {
            if (idle != null) idle.TickIdle();
            return;
        }

        if (IsStunned)
        {
            animator.SetBool("AnimIsWalking", false);
            motor.SetMovementLocked(true);
            motor.SetFacingLocked(true);
            return;
        }

        float playerDx = player.position.x - _lastPlayerPos.x;
        float playerSpeedX = Mathf.Abs(playerDx) / Mathf.Max(Time.deltaTime, 0.0001f);
        _lastPlayerPos = player.position;

        // Are they moving away from the enemy?
        float dirToPlayer = Mathf.Sign(player.position.x - transform.position.x);
        bool playerMovingAway =
            (dirToPlayer > 0f && playerDx > 0f) ||   // player to the right and moving right
            (dirToPlayer < 0f && playerDx < 0f);     // player to the left and moving left

        // If we are attacking, we NEVER move/turn until EndAttack.
        if (_isAttacking)
        {
            // Emergency escape only if animation event fails
            if (Time.time >= _attackFailsafeUntil)
            {
                StopAttackLocks();
                return;
            }

            animator.SetBool(PARAM_WALK, false);
            motor.SetMovementLocked(true);
            motor.SetFacingLocked(true);
            return;
        }

        if (Time.time < _postAttackUntil)
        {
            animator.SetBool(PARAM_WALK, false);
            motor.MoveHorizontally(0f, respectBlocks: false);
            return;
        }

        if (IsDisengaged)
        {
            idle.TickIdle();
            return;
        }

        float yDiff = Mathf.Abs(player.position.y - transform.position.y);
        if (yDiff > yChaseTolerance)
        {
            Disengage();
            idle.TickIdle();
            return;
        }

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist > aggroRange)
        {
            idle.TickIdle();
            return;
        }

        if (dist <= attackRange && Time.time >= _nextAttackTime)
        {
            StartAttack();
            return;
        }

        bool playerIsActuallyMoving = playerSpeedX > chaseWhenPlayerSpeedAbove;

        if (chaseOnlyIfSeparating)
        {
            if (!playerIsActuallyMoving || !playerMovingAway)
            {
                // Stand still (no shove). Still face the player if you want.
                animator.SetBool(PARAM_WALK, false);
                motor.MoveHorizontally(0f, respectBlocks: false);
                motor.SetFacing(dirToPlayer); // optional: face player while waiting
                return;
            }
        }
        else
        {
            // If you only want chase when player moves at all (not necessarily away):
            if (!playerIsActuallyMoving)
            {
                animator.SetBool(PARAM_WALK, false);
                motor.MoveHorizontally(0f, respectBlocks: false);
                return;
            }
        }

        ChasePlayer();
    }

    private void Disengage() => _disengageUntil = Time.time + disengageTime;

    private void ChasePlayer()
    {
        float dir = Mathf.Sign(player.position.x - transform.position.x);

        motor.SetFacing(dir);

        BlockReason reason = motor.GetForwardBlockReason();
        if (reason == BlockReason.Wall || reason == BlockReason.Cliff)
        {
            animator.SetBool(PARAM_WALK, false);
            motor.MoveHorizontally(0f, respectBlocks: false);
            Disengage();
            idle.TickIdle();
            return;
        }

        animator.SetBool(PARAM_WALK, true);
        motor.MoveHorizontally(dir * chaseSpeed);
    }

    private void StartAttack()
    {
        _nextAttackTime = Time.time + attackCooldown;

        // Enter attack immediately: locks apply from the first frame.
        _isAttacking = true;
        _attackFailsafeUntil = Time.time + attackFailsafeTime;

        animator.SetBool(PARAM_WALK, false);

        motor.SetMovementLocked(true);
        motor.SetFacingLocked(true);

        animator.ResetTrigger(TRIG_ATTACK);
        animator.SetTrigger(TRIG_ATTACK);
    }

    // Animation Event: call at hit frame
    public void DealDamage()
    {
        // Must NOT unlock or change movement here
        if (attackPoint == null) return;

        Collider2D hit = Physics2D.OverlapCircle(attackPoint.position, attackRadius, playerMask);
        if (hit == null) return;

        Hurtbox hurtbox = hit.GetComponent<Hurtbox>() ?? hit.GetComponentInParent<Hurtbox>();
        if (hurtbox != null)
        {
            hurtbox.TakeHit(attackDamage, transform);
            return;
        }

        Health h = hit.GetComponent<Health>() ?? hit.GetComponentInParent<Health>();
        if (h != null) h.ChangeHealth(-attackDamage);
    }

    public void StunInterrupt(float stunDuration)
    {
        // Stop attack immediately and lock behavior briefly
        EndAttack(); // unlocks motor & stops attack state in our final version
        _stunnedUntil = Time.time + stunDuration;
    }

    private bool IsStunned => Time.time < _stunnedUntil;


    // Animation Event: call at end of attack clip
    public void EndAttack()
    {
        StopAttackLocks();

        // Stand for a short moment so we don't shove the player
        _postAttackUntil = Time.time + postAttackStandTime;

        // Ensure walk bool is off immediately
        if (animator != null) animator.SetBool(PARAM_WALK, false);
        if (motor != null) motor.MoveHorizontally(0f, respectBlocks: false);
    }

    private void StopAttackLocks()
    {
        _isAttacking = false;
        motor.SetMovementLocked(false);
        motor.SetFacingLocked(false);
    }
}
