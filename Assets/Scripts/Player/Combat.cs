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

    public bool CanAttack => Time.time >= nextAttackTime;
    private float nextAttackTime;

    public void AttackAnimationFished()
    {
        player.AttackAnimationFished();
    }

    public void Attack()
    {
        if (!CanAttack) { return; }

        nextAttackTime = Time.time + attackCD;

        Collider2D enemy = Physics2D.OverlapCircle(attackPoint.position, attackRadius, enemyLayer);

        if (enemy != null)
        {
            enemy.gameObject.GetComponent<Health>().ChangeHealth(-damage);
        }
    }
}
