using UnityEngine;

public class PlayerStepUp : PlayerState
{
    private float _startTime;
    private Vector2 _startPos;
    private Vector2 _targetPos;

    // Cache values so it works the same for ground and air
    private float _up;
    private float _forward;
    private float _duration;

    public PlayerStepUp(Player player) : base(player) { }

    public override void Enter()
    {
        base.Enter();

        _startTime = Time.time;

        // Decide which "mode" we are using
        bool fromAir = !player.isGrounded && player.enableAirMantle;

        _up = fromAir ? player.mantleUp : player.stepHeight;
        _forward = fromAir ? player.mantleForward : player.stepForward;
        _duration = fromAir ? player.mantleDuration : player.stepDuration;

        // Freeze physics-ish for a clean animation move
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0f;

        animator.SetTrigger("stepUp"); // set your triggers

        _startPos = rb.position;
        _targetPos = _startPos + new Vector2(_forward * player.faceDir, _up);

        player.nextMantleTime = Time.time + player.mantleCooldown;
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        float t = Mathf.Clamp01((Time.time - _startTime) / Mathf.Max(_duration, 0.01f));
        Vector2 newPos = Vector2.Lerp(_startPos, _targetPos, t);
        rb.MovePosition(newPos);

        if (t >= 1f)
        {
            rb.gravityScale = player.normalG;

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
}
