using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class PlayerIdle : PlayerState
{

    public PlayerIdle (Player player) : base(player) { }

    public override void Enter()
    {
        animator.SetBool("isIdle", true);
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

    }

    public override void Update()
    {
        base.Update();

        if (HP <= 0)
        {
            player.ChangeState(player.deathState);
        }

        else if (AttackOnePressed && combat.CanAttack)
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

        // Auto step-up if a small ledge is in front
        else if (player.CanAutoStepUp())
        {
            player.ChangeState(player.stepUpState);
            return;
        }

        /*
        // Wall climb (press jump or W while touching wall)
        else if (player.IsTouchingClimbableWall() && player.climbPressed)
        {
            player.ChangeState(player.wallClimbState);
            return;
        }
        */

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
    }
}
