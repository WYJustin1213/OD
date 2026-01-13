using UnityEngine;

public class Combat : MonoBehaviour
{
    public Player player;

    [Header("Attack")]
    public int damage;
    public float attackRadius;
    public float attackCD;
    public Transform attackPoint;
    public LayerMask enemyLayer;

    [Header("Attack Type")]
    public PlayerAttackType attackType;

    public bool CanAttack => Time.time >= nextAttackTime;
    private float nextAttackTime;

    public void AttackAnimationFished()
    {
        player.AttackAnimationFished();
    }

    public void Attack()
    {
        if (!CanAttack) return;

        nextAttackTime = Time.time + attackCD;

        Collider2D hit = Physics2D.OverlapCircle(attackPoint.position, attackRadius, enemyLayer);
        if (hit == null) return;

        EnemyHurtbox hurtbox = hit.GetComponent<EnemyHurtbox>() ?? hit.GetComponentInParent<EnemyHurtbox>();
        if (hurtbox != null)
        {
            hurtbox.TakeHit(damage, attackType);
            return;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }
#endif
}
