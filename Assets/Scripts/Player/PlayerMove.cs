using UnityEngine;

public class PlayerMove : PlayerState
{
    public PlayerMove(Player player) : base(player) { }

    public override void Enter()
    {
        base.Enter();

        animator.SetBool("isRunning", !SprintPressed);
        animator.SetBool("isSprinting", SprintPressed);
    }


    public override void Update()
    {
        base.Update();

        if (AttackOnePressed && combat.CanAttack)
        {
            player.ChangeState(player.attackOneState);
        }
        else if (AttackTwoPressed && combat.CanAttack)
        {
            player.ChangeState(player.attackTwoState);
        }
        else if (AttackThreePressed && combat.CanAttack)
        {
            player.ChangeState(player.attackThreeState);
        }

        else if (JumpPressed)
        {
            player.ChangeState(player.jumpState);
        }
        else if (Mathf.Abs(MoveInput.x) < 0.01f)
        {
            player.ChangeState(player.idleState);
        }
        else if (player.isGrounded && SprintPressed && MoveInput.y < -0.01f)
        {
            player.ChangeState(player.slideState);
        }
        else if (MoveInput.y < -0.01f)
        {
            player.ChangeState(player.crouchState);
        }
        else
        {
            animator.SetBool("isRunning", !SprintPressed);
            animator.SetBool("isSprinting", SprintPressed);
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        float speed = SprintPressed ? player.sprintSpd : player.runSpd;
        rb.linearVelocity = new Vector2(speed * player.faceDir, rb.linearVelocity.y);
    }

    public override void Exit()
    {
        base.Exit();

        animator.SetBool("isRunning", false);
        animator.SetBool("isSprinting", false);

        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }
}
