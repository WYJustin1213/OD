using UnityEngine;

[RequireComponent(typeof(Health))]
public class EnemyCombatController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Health health;
    [SerializeField] private EnemyBrain brain;
    [SerializeField] private EnemyMotor2D motor;

    [Header("Player")]
    [SerializeField] private Transform player;

    [Header("Ranges")]
    [SerializeField] private float aggroRange = 8f;
    [SerializeField] private float attackRange = 1.2f;

    [Header("Movement")]
    [SerializeField] private float chaseSpeed = 2.5f;
    [SerializeField] private float hitStunTime = 0.15f;
    private float _hitStunEnd;

    [Header("Attack")]
    [SerializeField] private float attackCooldown = 1.0f;
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private Transform attackPoint;     // empty transform in front of enemy
    [SerializeField] private float attackRadius = 0.5f;
    [SerializeField] private LayerMask playerMask;

    private float _nextAttackTime;
    private bool _isAttacking;

    private int AnimIsWalking = Animator.StringToHash("isWalking");
    private int AnimAttack = Animator.StringToHash("attack");

    private void Reset()
    {
        health = GetComponent<Health>();
    }

    private void Awake()
    {
        if (health == null) health = GetComponent<Health>();
        if (motor == null) motor = GetComponent<EnemyMotor2D>();
        if (brain == null) brain = GetComponent<EnemyBrain>();
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
        if (health != null)
        {
            health.OnDeath += HandleDeath;
            health.OnDamaged += OnDamaged;
        }
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.OnDeath -= HandleDeath;
            health.OnDamaged -= OnDamaged;
        }
    }

    private void OnDamaged()
    {
        _hitStunEnd = Time.time + hitStunTime;
    }

    private void HandleDeath()
    {
        // Stop moving instantly on death
        if (motor != null) motor.MoveHorizontally(0f);
        enabled = false;
    }

    private void Update()
    {
        if (player == null || _isAttacking) return;

        if (Time.time < _hitStunEnd)
        {
            motor.MoveHorizontally(0f);
            animator.SetBool(AnimIsWalking, false);
            return;
        }

        float distance = Vector2.Distance(transform.position, player.position);

        // Not aggro: idle
        if (distance > aggroRange)
        {
            animator.SetBool("AnimIsWalking", false);
            motor.MoveHorizontally(0f);
            return;
        }

        // In attack range: attack
        if (distance <= attackRange && Time.time >= _nextAttackTime)
        {
            StartAttack();
            return;
        }

        // Otherwise: chase (using brain intent if you want universe influence)
        Chase(distance);
    }

    private void Chase(float dist)
    {
        // Option 1 (simple): direct chase toward player
        float dir = Mathf.Sign(player.position.x - transform.position.x);

        // Option 2 (universe influence): use brain’s intent direction when chasing
        // If your EnemyBrain currently outputs rb.velocity directly, change it to output intent only.
        // Here, we assume brain exists and we just use its direction preference when it’s non-zero.
        if (brain != null)
        {
            // Brain in our earlier skeleton sets rb.velocity itself.
            // Recommended upgrade: modify EnemyBrain so it exposes last intent instead of setting rb directly.
            // For now, we keep chase simple.
        }

        animator.SetBool("AnimIsWalking", true);
        motor.SetFacing(dir);
        motor.MoveHorizontally(dir * chaseSpeed);
    }

    private void StartAttack()
    {
        _isAttacking = true;
        _nextAttackTime = Time.time + attackCooldown;

        animator.SetBool("AnimIsWalking", false);
        motor.MoveHorizontally(0f);

        animator.SetTrigger("AnimAttack");
        // Damage will be applied by an Animation Event calling DealDamage()
        // (recommended), or by a timed coroutine (less accurate).
    }

    /// <summary>
    /// Call this from an Animation Event at the frame you want the hit to land.
    /// </summary>
    public void DealDamage()
    {
        if (attackPoint == null) return;

        Collider2D hit = Physics2D.OverlapCircle((Vector2)attackPoint.position, attackRadius, playerMask);
        if (hit == null) return;

        Health playerHealth = hit.GetComponent<Health>();
        if (playerHealth != null)
        {
            playerHealth.ChangeHealth(-attackDamage);
        }
    }

    /// <summary>
    /// Call this from an Animation Event at the end of the attack animation.
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
