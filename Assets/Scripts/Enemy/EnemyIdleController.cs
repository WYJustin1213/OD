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

    [Header("Turn Delay")]
    [SerializeField] private float turnDelay = 1f; // pause before turning
    private bool _turnPending;
    private float _turnAtTime;

    
    private float _nextTurnTime;

    private int AnimIsWalking = Animator.StringToHash("AnimIsWalking");

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

    private void RequestTurn()
    {
        if (_turnPending) return;
        _turnPending = true;
        _turnAtTime = Time.time + turnDelay;

        // Stop while preparing to turn
        SetWalking(false);
        motor.MoveHorizontally(0f);
    }

    public void TickIdle()
    {
        if (motor == null) return;

        if (motor != null && motor.IsMovementLocked)
        {
            SetWalking(false);
            motor.MoveHorizontally(0f, respectBlocks: false);
            return;
        }

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
                    // If we're waiting to turn, stand still until the delay finishes
                    if (_turnPending)
                    {
                        if (Time.time >= _turnAtTime)
                        {
                            motor.SetFacing(-motor.Facing);
                            _turnPending = false;
                            _nextTurnTime = Time.time + walkDuration;
                        }

                        SetWalking(false);
                        motor.MoveHorizontally(0f);
                        break;
                    }

                    BlockReason reason = motor.GetForwardBlockReason();

                    // If blocked (cliff/wall/same-type enemy), request a delayed turn
                    if (reason != BlockReason.None)
                    {
                        RequestTurn();
                        break;
                    }

                    // Timed turn
                    if (Time.time >= _nextTurnTime)
                    {
                        RequestTurn();
                        break;
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
