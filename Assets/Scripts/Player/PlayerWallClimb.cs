using UnityEngine;

public class PlayerWallClimb : PlayerState
{
    public PlayerWallClimb(Player player) : base(player) { }
    /*
    private float _startTime;
    private Vector2 _startPos;
    private Vector2 _targetPos;

    public override void Enter()
    {
        base.Enter();

        // Consume jump so you don't also jump
        JumpPressed = false;

        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0f;

        animator.SetTrigger("wallClimb"); // your animation trigger name

        _startTime = Time.time;
        _startPos = rb.position;

        float forward = player.wallClimbForward * player.faceDir;
        _targetPos = _startPos + new Vector2(forward, player.wallClimbUp);
    }

    public override void Update()
    {
        base.Update();

        if (JumpPressed)
        {
            JumpPressed = false;
            player.ChangeState(player.jumpState);
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        float t = Mathf.Clamp01((Time.time - _startTime) / player.wallClimbDuration);
        rb.MovePosition(Vector2.Lerp(_startPos, _targetPos, t));

        if (t >= 1f)
        {
            // restore gravity
            rb.gravityScale = player.normalG;

            // Continue based on input
            if (Mathf.Abs(player.moveInput.x) > 0.01f)
                player.ChangeState(player.moveState);
            else
                player.ChangeState(player.idleState);
        }
    }

    public override void Exit()
    {
        base.Exit();
        rb.gravityScale = player.normalG;
    }
    */
}
