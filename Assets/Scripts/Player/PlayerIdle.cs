using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class PlayerIdle : PlayerState
{

    public PlayerIdle (Player player) : base(player) { }

    public override void Enter()
    {
        animator.SetBool("isIdle", true);
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        animator.SetBool("isRunning", false);
        animator.SetBool("isSprinting", false);
    }

    public override void Update()
    {
        base.Update();

        if (AttackOnePressed && combat.CanAttack)
        {
            player.ChangeState(player.attackOneState);
        }
        else if (JumpPressed)
        {
            JumpPressed = false;
            player.ChangeState(player.jumpState);
            
        }
        else if (Mathf.Abs(player.moveInput.x) > 0.01f)
        {
            player.ChangeState(player.moveState);
        }
        else if (MoveInput.y < -0.01f)
        {
            player.ChangeState(player.crouchState);
        }
    }

    public override void Exit()
    {
        animator.SetBool("isIdle", false);
        Debug.Log("Leave");
    }
}
