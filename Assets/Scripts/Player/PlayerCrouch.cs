using UnityEngine;

public class PlayerCrouch : PlayerState
{
    public PlayerCrouch(Player player) : base(player) { }

    public override void Enter()
    {
        base.Enter();

        animator.SetBool("isCrouching", true);

        player.SetColliderSlide();

        animator.SetBool("isRunning", false);
        animator.SetBool("isSprinting", false);
    }

    public override void Update()
    {
        base.Update();

        if (MoveInput.y > -0.01f && !player.CheckForCeiling())
        {
            player.ChangeState(player.idleState);
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        if (Mathf.Abs(MoveInput.x) > 0.01f)
        {
            rb.linearVelocity = new Vector2(player.faceDir * player.runSpd, rb.linearVelocity.y);
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    public override void Exit()
    {
        base.Exit();

        animator.SetBool("isCrouching", false);

        player.SetColliderNormal();
    }
}
