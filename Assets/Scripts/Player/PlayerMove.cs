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

        if (HP <= 0)
        {
            player.ChangeState(player.deathState);
        }

        else if (AttackOnePressed && combat.CanAttack)
        {
            if (player.stamina != null && player.stamina.TrySpend(player.attackOneCost))
                player.ChangeState(player.attackOneState);
            return;
        }

        else if (AttackTwoPressed && combat.CanAttack)
        {
            if (player.stamina != null && player.stamina.TrySpend(player.attackTwoCost))
                player.ChangeState(player.attackTwoState);
            return;
        }

        else if (AttackThreePressed && combat.CanAttack)
        {
            if (player.stamina != null && player.stamina.TrySpend(player.attackThreeCost))
                player.ChangeState(player.attackThreeState);
            return;
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
            player.ChangeState(player.jumpState);
        }
        else if (Mathf.Abs(MoveInput.x) < 0.01f)
        {
            player.ChangeState(player.idleState);
        }
        else if (player.isGrounded && SprintPressed && MoveInput.y < -0.01f)
        {
            if (player.stamina != null && player.stamina.TrySpend(player.slideCost))
                player.ChangeState(player.slideState);
            return;
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

        float baseSpeed = SprintPressed ? player.sprintSpd : player.runSpd;
        float cappedSpeed = player.GetCappedSpeed(baseSpeed);

        rb.linearVelocity = new Vector2(cappedSpeed * player.faceDir, rb.linearVelocity.y);
    }

    public override void Exit()
    {
        base.Exit();

        animator.SetBool("isRunning", false);
        animator.SetBool("isSprinting", false);

        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }
}
