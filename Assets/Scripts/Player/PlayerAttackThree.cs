using UnityEngine;

public class PlayerAttackThree : PlayerState
{
    public PlayerAttackThree(Player player) : base(player) { }


    public override void Enter()
    {
        base.Enter();

        animator.SetBool("isAttacking3", true);

        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    public override void AttackAnimationFished()
    {
        
        player.ChangeState(player.crouchState);
    }

    public override void Exit()
    {
        base.Exit();

        animator.SetBool("isAttacking3", false);
    }
}
