using UnityEngine;

public class PlayerSlide : PlayerState
{
    private float slideTimer;
    private float slideStopTimer;

    public PlayerSlide(Player player) : base(player) { }

    public override void Enter()
    {
        base.Enter();

        slideTimer = player.slideDuration;
        slideStopTimer = 0;

        player.SetColliderSlide();
        animator.SetBool("isSliding", true);

        animator.SetBool("isRunning", false);
        animator.SetBool("isSprinting", false);
    }

    public override void Update()
    {
        base.Update();

        if (slideTimer > 0)
        {
            slideTimer -= Time.deltaTime;
        }
        else if (slideStopTimer <= 0)
        {
            slideStopTimer = player.slideStopDur;
        }
        else
        {
            slideStopTimer -= Time.deltaTime;

            if (slideStopTimer <= 0)
            {
                if (player.CheckForCeiling() || MoveInput.y <= -0.01f)
                {
                    player.ChangeState(player.crouchState);
                }
                else
                {
                    player.ChangeState(player.idleState);
                }
            }
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        if (slideTimer > 0)
        {
            rb.linearVelocity = new Vector2(player.slideSpd * player.faceDir, rb.linearVelocity.y);
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    public override void Exit()
    {
        base.Exit();

        player.SetColliderNormal();
        animator.SetBool("isSliding", false);
    }
}
