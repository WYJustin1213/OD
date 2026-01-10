using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class PlayerAttackOne : PlayerState
{
    public PlayerAttackOne(Player player) : base(player) { }


    public override void Enter()
    {
        base.Enter();

        animator.SetBool("isAttacking1", true);

        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    public override void AttackAnimationFished()
    {
        if (Mathf.Abs(MoveInput.x) > 0.01f)
        {
            player.ChangeState(player.moveState);
        }
        else
        {
            player.ChangeState(player.idleState);
        }
    }

    public override void Exit()
    {
        base.Exit();

        animator.SetBool("isAttacking1", false);

        rb.position = new Vector2(rb.position.x + 0.45f * player.faceDir, rb.position.y);
    }
}
