using UnityEngine;

public enum EnemyIdleMode
{
    StandStill,
    WalkOneWayUntilBlocked,
    WalkTimedTurn
}

public class EnemyIdleController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private EnemyMotor2D motor;
    [SerializeField] private Animator animator; // injected by variant switcher
    [SerializeField] private float idleWalkSpeed = 1.2f;

    [Header("Idle Mode")]
    [SerializeField] private EnemyIdleMode idleMode = EnemyIdleMode.StandStill;

    [Header("WalkTimedTurn Settings")]
    [SerializeField] private float walkDuration = 2.0f;

    [Header("Initial Direction")]
    [SerializeField] private float initialFacing = 1f; // +1 right, -1 left

    private float _nextTurnTime;

    private int AnimIsWalking = Animator.StringToHash("isWalking");

    private void Awake()
    {
        if (motor == null) motor = GetComponent<EnemyMotor2D>();
        motor.SetFacing(initialFacing);
        _nextTurnTime = Time.time + walkDuration;
    }

    public EnemyIdleMode CurrentMode => idleMode;

    public void SetAnimator(Animator a) => animator = a;

    public void SetIdleMode(EnemyIdleMode mode)
    {
        idleMode = mode;
        // Reset timers so changing modes mid-game feels consistent
        _nextTurnTime = Time.time + walkDuration;
    }

    public void TickIdle()
    {
        if (motor == null) return;

        switch (idleMode)
        {
            case EnemyIdleMode.StandStill:
                SetWalking(false);
                motor.MoveHorizontally(0f);
                break;

            case EnemyIdleMode.WalkOneWayUntilBlocked:
                {
                    BlockReason reason = motor.GetForwardBlockReason();
                    if (reason != BlockReason.None)
                    {
                        // Stop permanently once blocked
                        SetWalking(false);
                        motor.MoveHorizontally(0f);
                    }
                    else
                    {
                        SetWalking(true);
                        motor.MoveHorizontally(motor.Facing * idleWalkSpeed);
                    }
                    break;
                }

            case EnemyIdleMode.WalkTimedTurn:
                {
                    BlockReason reason = motor.GetForwardBlockReason();

                    // If blocked, turn around immediately
                    if (reason != BlockReason.None)
                    {
                        TurnAround();
                        SetWalking(true);
                        motor.MoveHorizontally(motor.Facing * idleWalkSpeed);
                        _nextTurnTime = Time.time + walkDuration;
                        break;
                    }

                    // Normal timed turn
                    if (Time.time >= _nextTurnTime)
                    {
                        TurnAround();
                        _nextTurnTime = Time.time + walkDuration;
                    }

                    SetWalking(true);
                    motor.MoveHorizontally(motor.Facing * idleWalkSpeed);
                    break;
                }
        }
    }

    private void TurnAround()
    {
        motor.SetFacing(-motor.Facing);
    }

    private void SetWalking(bool walking)
    {
        if (animator != null)
            animator.SetBool(AnimIsWalking, walking);
    }
}
