using UnityEngine;

public class PlayerStepUp : PlayerState
{
    private float _startTime;
    private Vector2 _startPos;
    private Vector2 _targetPos;

    public PlayerStepUp(Player player) : base(player) { }

    public override void Enter()
    {
        base.Enter();

        _startTime = Time.time;

        // lock horizontal movement
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        animator.SetTrigger("stepUp"); 

        _startPos = rb.position;

        float forward = player.stepForward * player.faceDir;
        _targetPos = _startPos + new Vector2(forward, player.stepHeight);
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        float t = Mathf.Clamp01((Time.time - _startTime) / player.stepDuration);

        // Smooth step movement
        Vector2 newPos = Vector2.Lerp(_startPos, _targetPos, t);
        rb.MovePosition(newPos);

        if (t >= 1f)
        {
            // Finish by returning to Move or Idle depending on input
            if (Mathf.Abs(player.moveInput.x) > 0.01f)
                player.ChangeState(player.moveState);
            else
                player.ChangeState(player.idleState);
        }
    }

    public override void Exit()
    {
        base.Exit();
        // nothing special
    }
}
