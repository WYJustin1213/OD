using UnityEngine;

[RequireComponent(typeof(Health))]
public class EnemyCombatController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Health health;
    [SerializeField] private EnemyMotor2D motor;

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

    // These are now VARIANT-INJECTED
    private Animator animator;
    private Transform attackPoint;

    private float _nextAttackTime;
    private bool _isAttacking;
    private float _attackUnlockTime;

    [SerializeField] private float attackLockTime = 0.6f; // set to your attack anim length

    private int AnimIsWalking = Animator.StringToHash("isWalking");
    private int AnimAttack = Animator.StringToHash("attack");

    private void Awake()
    {
        if (health == null) health = GetComponent<Health>();
        if (motor == null) motor = GetComponent<EnemyMotor2D>();
    }

    private void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
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

    /// <summary>
    /// Called by UniverseEnemyVariantSwitcher whenever the universe changes.
    /// </summary>
    public void ApplyVariantRefs(EnemyVariantCombatRefs refs)
    {
        if (refs == null)
        {
            Debug.LogWarning($"EnemyCombatController on {name}: received null variant refs.", this);
            animator = null;
            attackPoint = null;
            return;
        }

        animator = refs.animator;
        attackPoint = refs.attackPoint;

        if (refs.attackRangeOverride > 0f) attackRange = refs.attackRangeOverride;
        if (refs.attackRadiusOverride > 0f) attackRadius = refs.attackRadiusOverride;
    }

    private void HandleDeath()
    {
        if (motor != null) motor.MoveHorizontally(0f);
        enabled = false;
    }

    private void Update()
    {
        // failsafe unlock
        if (_isAttacking && Time.time >= _attackUnlockTime)
            _isAttacking = false;

        if (player == null) return;

        // If refs not injected yet, just idle
        if (animator == null || attackPoint == null)
        {
            if (motor != null) motor.MoveHorizontally(0f);
            return;
        }

        if (_isAttacking) return;

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist > aggroRange)
        {
            animator.SetBool(AnimIsWalking, false);
            motor.MoveHorizontally(0f);
            return;
        }

        if (dist <= attackRange && Time.time >= _nextAttackTime)
        {
            StartAttack();
            return;
        }

        Chase();
    }

    private void Chase()
    {
        float dir = Mathf.Sign(player.position.x - transform.position.x);

        animator.SetBool(AnimIsWalking, true);
        motor.SetFacing(dir);
        motor.MoveHorizontally(dir * chaseSpeed);
    }

    private void StartAttack()
    {
        _isAttacking = true;
        _attackUnlockTime = Time.time + attackLockTime;
        _nextAttackTime = Time.time + attackCooldown;

        animator.SetBool(AnimIsWalking, false);
        motor.MoveHorizontally(0f);

        animator.SetTrigger(AnimAttack);
    }

    /// <summary>
    /// Animation Event: call at hit frame.
    /// </summary>
    public void DealDamage()
    {
        if (attackPoint == null) return;

        Collider2D hit = Physics2D.OverlapCircle((Vector2)attackPoint.position, attackRadius, playerMask);
        if (hit == null) return;

        Health playerHealth = hit.GetComponent<Health>();
        if (playerHealth != null)
            playerHealth.ChangeHealth(-attackDamage);
    }

    /// <summary>
    /// Animation Event: call at end of attack clip.
    /// </summary>
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
