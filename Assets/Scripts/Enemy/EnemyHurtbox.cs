using UnityEngine;

public class EnemyHurtbox : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Health health;
    [SerializeField] private Enemy enemy;                 // your hit animation handler (animator.SetTrigger("isHit"))
    [SerializeField] private EnemyCombatController combat; // to check if attacking + to stun/cancel attack

    private void Awake()
    {
        if (health == null) health = GetComponentInParent<Health>();
        if (enemy == null) enemy = GetComponentInParent<Enemy>();
        if (combat == null) combat = GetComponentInParent<EnemyCombatController>();
    }

    /// <summary>
    /// Called by player attacks.
    /// </summary>
    public void TakeHit(int damage, PlayerAttackType attackType)
    {
        if (health != null)
            health.ChangeHealth(-Mathf.Abs(damage));

        bool enemyIsAttacking = (combat != null && combat.IsAttacking);

        bool shouldStun =
            (attackType == PlayerAttackType.AttackOne || attackType == PlayerAttackType.AttackThree)
                ? !enemyIsAttacking
                : (attackType == PlayerAttackType.AttackTwo)
                    ? enemyIsAttacking
                    : false;

        if (shouldStun)
        {
            // Show stun as hit animation
            if (enemy != null) enemy.PlayHit();   // we’ll add this helper if you don’t have it
            //else if (combat != null) combat.TriggerHitAnimationFallback();

            // Cancel / interrupt combat if needed
            if (combat != null)
                combat.StunInterrupt(0.25f); // small stun window; tune as you like
        }
    }
}
