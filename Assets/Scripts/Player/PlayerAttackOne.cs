using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class PlayerAttackOne : PlayerState
{
    public PlayerAttackOne(Player player) : base(player) { }


    public override void Enter()
    {
        base.Enter();

        animator.SetBool("isAttacking1", true);
        combat.attackType = PlayerAttackType.AttackOne;
        combat.damage = AttackOneDamage;

        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    public override void Update()
    {
        base.Update();

        if (HP <= 0)
        {
            player.ChangeState(player.deathState);
        }
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

        if (Mathf.Abs(MoveInput.x) < 0.01f)
        {
            float stepUp = 0.5f * player.faceDir;
            rb.position = new Vector2(rb.position.x + stepUp, rb.position.y);
        }
    }
}
