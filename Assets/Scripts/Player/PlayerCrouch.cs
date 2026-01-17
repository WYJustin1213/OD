using UnityEngine;

public class PlayerCrouch : PlayerState
{
    public PlayerCrouch(Player player) : base(player) { }

    public override void Enter()
    {
        base.Enter();

        animator.SetBool("isCrouching", true);

        player.SetColliderSlide();
    }

    public override void Update()
    {
        base.Update();

        if (HP <= 0)
        {
            player.ChangeState(player.deathState);
        }

        if (MoveInput.y > -0.01f && !player.CheckForCeiling())
        {
            player.ChangeState(player.idleState);
        }

        bool isGrounded = animator.GetBool("isGrounded");
        bool isMoving = Mathf.Abs(MoveInput.x) > 0.01f && isGrounded;

        animator.SetBool("isCrouchWalking", isMoving);

        if (AttackThreePressed && combat.CanAttack)
        {
            animator.SetBool("isCrouchWalking", false);
            player.ChangeState(player.attackThreeState);
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
