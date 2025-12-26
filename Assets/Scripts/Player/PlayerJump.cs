using UnityEngine;
using UnityEngine.LowLevel;

public class PlayerJump : PlayerState
{
    public PlayerJump(Player player) : base(player) { }

    public override void Enter()
    {
        base.Enter();

        animator.SetBool("isJumping", true);

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, player.jumpForce);

        JumpPressed = false;
    }

    public override void Update()
    {
        base.Update();

        if (player.isGrounded && rb.linearVelocity.y <= 0)
        {
            player.ChangeState(player.idleState);
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        player.ApplyVariableGravity();

        float speed = SprintPressed ? player.sprintSpd : player.runSpd;
        float targetSpd = speed * MoveInput.x;
        rb.linearVelocity = new Vector2(targetSpd, rb.linearVelocity.y);
    }

    public override void Exit()
    {
        base.Exit();

        animator.SetBool("isJumping", false);
    }
}
