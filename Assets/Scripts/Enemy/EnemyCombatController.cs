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

    [Header("Attack timing")]
    [SerializeField] private float attackLockTime = 0.6f; // anim length (failsafe)

    // Variant-injected
    private Animator animator;
    private Transform attackPoint;

    private float _nextAttackTime;
    private bool _isAttacking;
    private float _attackUnlockTime;

    private int AnimIsWalking = Animator.StringToHash("isWalking");
    private int AnimAttack = Animator.StringToHash("attack");

    [Header("Disengage (prevents flip-flop when player is unreachable)")]
    [SerializeField] private float disengageTime = 1.0f;      // how long to stop chasing after blocked
    [SerializeField] private float yChaseTolerance = 0.9f;     // if player is too high/low, treat as unreachable

    private float _disengageUntil;

    private bool IsDisengaged => Time.time < _disengageUntil;

    private void Disengage()
    {
        _disengageUntil = Time.time + disengageTime;
    }

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
        motor.MoveHorizontally(0f);
        enabled = false;
    }

    private void Update()
    {
        // failsafe unlock
        if (_isAttacking && Time.time >= _attackUnlockTime)
            _isAttacking = false;

        // If we don't have refs yet, just idle
        if (animator == null || motor == null || idle == null || player == null)
        {
            if (idle != null) idle.TickIdle();
            return;
        }

        // During attack, motor is stopped (hit layer can still play)
        if (_isAttacking) return;

        // If we recently hit a cliff/wall or the player is unreachable, just idle for a bit.
        if (IsDisengaged)
        {
            idle.TickIdle();
            return;
        }

        float dist = Vector2.Distance(transform.position, player.position);
        float yDiff = Mathf.Abs(player.position.y - transform.position.y);
        if (yDiff > yChaseTolerance)
        {
            // Player likely on another platform; without pathfinding, don't spam chase.
            Disengage();
            idle.TickIdle();
            return;
        }

        if (dist > aggroRange)
        {
            idle.TickIdle();
            return;
        }

        // If close enough, attack
        if (dist <= attackRange && Time.time >= _nextAttackTime)
        {
            StartAttack();
            return;
        }

        // Chase
        ChasePlayer();
    }

    private void ChasePlayer()
    {
        float dir = Mathf.Sign(player.position.x - transform.position.x);

        // We do NOT force facing if we’re going to disengage this frame.
        // Check block reason first using current intended facing:
        motor.SetFacing(dir);
        BlockReason reason = motor.GetForwardBlockReason();

        // If blocked by wall/cliff, give up chase temporarily and return to idle.
        if (reason == BlockReason.Wall || reason == BlockReason.Cliff)
        {
            animator.SetBool("AnimIsWalking", false);
            motor.MoveHorizontally(0f);

            Disengage();          // <-- key line: prevents next frame from re-chasing immediately
            idle.TickIdle();
            return;
        }

        // Same-type enemy block: keep trying (or you could idle, but you asked not to disengage for this case)
        animator.SetBool("AnimIsWalking", true);
        motor.MoveHorizontally(dir * chaseSpeed);
    }


    private void StartAttack()
    {
        _isAttacking = true;
        _attackUnlockTime = Time.time + attackLockTime;
        _nextAttackTime = Time.time + attackCooldown;

        animator.SetBool("AnimIsWalking", false);
        motor.MoveHorizontally(0f);

        animator.SetTrigger("AnimAttack");
    }

    // Animation Event on the variant animator (use a proxy if needed)
    public void DealDamage()
    {
        if (attackPoint == null) return;

        Collider2D hit = Physics2D.OverlapCircle((Vector2)attackPoint.position, attackRadius, playerMask);
        if (hit == null) return;

        Health playerHealth = hit.GetComponent<Health>();
        if (playerHealth != null)
            playerHealth.ChangeHealth(-attackDamage);
    }

    // Animation Event at end of clip
    public void EndAttack()
    {
        _isAttacking = false;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }
#endif
}
