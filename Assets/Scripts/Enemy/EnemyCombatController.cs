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
    public Player playerScript;

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

    public bool IsTargetingPlayer => _isTargetingPlayer;

    private bool _isTargetingPlayer;

    private float _postAttackUntil;

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
        }
    }

    private void OnEnable()
    {
        if (health != null) health.OnDeath += HandleDeath;
    }

    private void OnDisable()
    {
        if (_isTargetingPlayer && BattleTracker.Instance != null)
            BattleTracker.Instance.NotifyTargetingStopped();

        if (health != null) health.OnDeath -= HandleDeath;
    }

    private void SetTargeting(bool targeting)
    {
        if (_isTargetingPlayer == targeting) return;
        _isTargetingPlayer = targeting;

        if (BattleTracker.Instance == null) return;

        if (targeting) BattleTracker.Instance.NotifyTargetingStarted();
        else BattleTracker.Instance.NotifyTargetingStopped();
    }

    public void ApplyVariantAnimatorAndAttackPoint(Animator a, Transform ap)
    {
        animator = a;
        attackPoint = ap;
        if (idle != null) idle.SetAnimator(a);
    }

    private void HandleDeath()
    {
        SetTargeting(false); // ✅ ensure battle tracker decrements now
        StopAttackLocks();
        motor.MoveHorizontally(0f, respectBlocks: false);
        enabled = false;
    }


    private void Update()
    {
        bool shouldTarget = false; // we will set this true only if we truly want battle music on

        try
        {
            // Missing refs -> cannot target
            if (player == null)
            {
                if (idle != null) idle.TickIdle();
                return;
            }

            // If player dead -> cannot target
            if (playerScript != null && playerScript.isDead)
            {
                StopAttackLocks();
                if (idle != null) idle.TickIdle();
                return;
            }

            if (animator == null || motor == null || idle == null)
            {
                if (idle != null) idle.TickIdle();
                return;
            }

            // Basic checks we use both for AI & battle state
            float dist = Vector2.Distance(transform.position, player.position);
            float yDiff = Mathf.Abs(player.position.y - transform.position.y);

            // Out of aggro or unreachable -> not targeting
            if (dist > aggroRange || yDiff > yChaseTolerance)
            {
                StopAttackLocks();
                if (yDiff > yChaseTolerance) Disengage();
                idle.TickIdle();
                return;
            }

            // Disengaged -> not targeting
            if (IsDisengaged)
            {
                idle.TickIdle();
                return;
            }

            // Stunned -> not targeting
            if (IsStunned)
            {
                animator.SetBool(PARAM_WALK, false);
                motor.SetMovementLocked(true);
                motor.SetFacingLocked(true);
                return;
            }

            // ✅ If we reach here, enemy is actively targeting player (in aggro & reachable & not disengaged/stunned)
            shouldTarget = true;

            // ---- Your existing combat/chase logic below ----
            float dx = Mathf.Abs(player.position.x - transform.position.x);

            if (_isAttacking)
            {
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

            // Post-attack hold
            if (Time.time < _postAttackUntil)
            {
                animator.SetBool(PARAM_WALK, false);
                motor.SetMovementLocked(true);
                motor.SetFacingLocked(false);
                motor.SetFacing(Mathf.Sign(player.position.x - transform.position.x));
                return;
            }

            // In attack range
            if (dx <= attackRange)
            {
                animator.SetBool(PARAM_WALK, false);
                motor.SetMovementLocked(true);
                motor.SetFacingLocked(false);
                motor.SetFacing(Mathf.Sign(player.position.x - transform.position.x));

                if (Time.time >= _nextAttackTime)
                    StartAttack();

                return;
            }

            // Chase
            StopAttackLocks();
            ChasePlayer();
        }
        finally
        {
            // ✅ Always keep BattleTracker correct
            SetTargeting(shouldTarget);
        }
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

        ShakeBus.Instance?.EnemyAttack();
    }

    private void ClearTargeting()
    {
        SetTargeting(false);
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
        _isAttacking = false;

        // Hold position after attack
        _postAttackUntil = Time.time + postAttackStandTime;

        if (animator != null) animator.SetBool(PARAM_WALK, false);

        // Keep movement locked during the hold
        motor.SetMovementLocked(true);
        motor.SetFacingLocked(false);
    }


    private void StopAttackLocks()
    {
        _isAttacking = false;
        motor.SetMovementLocked(false);
        motor.SetFacingLocked(false);
    }
}
