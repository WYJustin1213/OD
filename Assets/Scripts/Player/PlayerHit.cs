using UnityEngine;

public class PlayerHit : PlayerState
{
    // Config (tune these in Player if you want; keeping here for simplicity)
    private float hitStateMinTime = 0.15f; // prevents instant cancel if animation event fails
    private float enterTime;

    private Transform attacker;

    // Animator hashes (must exist in your Animator Controller)
    private readonly int HitTrig = Animator.StringToHash("isHit");

    public PlayerHit(Player player) : base(player) { }

    public void SetAttacker(Transform attackerTransform)
    {
        attacker = attackerTransform;
    }

    public override void Enter()
    {
        base.Enter();

        player.inputLocked = true;

        enterTime = Time.time;

        // Stop current motion (optional)
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        // Trigger hit animation
        animator.SetTrigger(HitTrig);

        // Apply knockback only if attacker is a Large enemy
        if (attacker != null)
        {
            EnemyIdentity enemyId = attacker.GetComponentInParent<EnemyIdentity>();
            if (enemyId != null && enemyId.SizeType == EnemySizeType.Large)
            {
                ApplyKnockbackFrom(attacker.position.x);
            }
        }

        // Optional: prevent immediate re-hit by disabling combat for a moment, if you have that system
        //combat.CanAttack = false; // only if your Combat script supports it
    }

    public override void Update()
    {
        base.Update();

        // Optional: apply variable gravity while in hit (so it still falls correctly)
        player.ApplyVariableGravity();
    }

    private void ApplyKnockbackFrom(float attackerX)
    {
        // Direction: knock away from attacker
        float dir = (player.transform.position.x >= attackerX) ? 1f : -1f;

        // TUNING: you can expose these on Player later
        float kbX = 17f;
        float kbY = 3f;

        // Clear current x vel so the knockback feels consistent
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        rb.AddForce(new Vector2(dir * kbX, kbY), ForceMode2D.Impulse);
    }

    public override void AttackAnimationFished()
    {
        // We reuse your existing animation-event callback.
        // Make sure your hit animation clip calls Player.AttackAnimationFished() at the end.
        if (Time.time - enterTime < hitStateMinTime)
            return;

        // Decide what state to go to:
        if (!player.isGrounded)
            player.ChangeState(player.jumpState);
        else if (Mathf.Abs(player.moveInput.x) > 0.01f)
            player.ChangeState(player.moveState);
        else
            player.ChangeState(player.idleState);
    }

    public override void Exit()
    {
        base.Exit();
        attacker = null;
        player.inputLocked = false;

    }
}
