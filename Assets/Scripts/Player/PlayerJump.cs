using UnityEngine;
using UnityEngine.LowLevel;

public class PlayerJump : PlayerState
{
    public PlayerJump(Player player) : base(player) { }

    public override void Enter()
    {
        base.Enter();

        animator.SetBool("isJumping", true);
        animator.SetBool("isGrounded", false);
        Debug.Log("jump");
        animator.SetTrigger("Jump");

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, player.jumpForce);

        JumpPressed = false;
        
    }

    public override void Update()
    {
        base.Update();

        animator.SetBool("isSprinting", false);

        // Auto step-up if a small ledge is in front
        if (player.CanAirMantle())
        {
            player.ChangeState(player.stepUpState);
            return;
        }

        if (player.isGrounded && rb.linearVelocity.y <= 0)
        {
            if (Mathf.Abs(player.moveInput.x) > 0.01f)
            {
                player.ChangeState(player.moveState);
            }
            else if (Mathf.Abs(MoveInput.x) < 0.01f)
            {
                player.ChangeState(player.idleState);
            }
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
