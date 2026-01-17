using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class PlayerDeath : PlayerState
{
    public PlayerDeath(Player player) : base(player) { }


    public override void Enter()
    {
        base.Enter();

        animator.SetTrigger("isDead");

        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        //player.playerCollider.enabled = false;
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    public override void Exit()
    {
        base.Exit();
    }
}
