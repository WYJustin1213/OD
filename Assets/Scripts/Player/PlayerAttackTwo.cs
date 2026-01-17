using UnityEngine;

public class PlayerAttackTwo : PlayerState
{
    public PlayerAttackTwo(Player player) : base(player) { }


    public override void Enter()
    {
        base.Enter();

        animator.SetBool("isAttacking2", true);
        combat.attackType = PlayerAttackType.AttackTwo;
        combat.damage = AttackTwoDamage;

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

        animator.SetBool("isAttacking2", false);
    }
}
