using UnityEngine;

public class PlayerLocomotionLoopSfx : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Player player;
    [SerializeField] private Rigidbody2D rb;

    [Header("Loop Sources (2D)")]
    [SerializeField] private AudioSource walkLoop;
    [SerializeField] private AudioSource runLoop;

    [Header("Tuning")]
    [SerializeField] private float fadeSpeed = 10f;          // higher = snappier fades
    [SerializeField] private float minMoveSpeed = 0.2f;      // ignore tiny jitter
    [SerializeField] private float walkMaxSpeed = 3.0f;      // above this, treat as run (if you want speed-based)
    [SerializeField] private bool useSprintFlag = true;      // if true: sprintPressed decides run vs walk

    [Header("Gating")]
    [SerializeField] private bool onlyWhenGrounded = true;
    [SerializeField] private bool muteDuringAttacks = true;  // optional (if you want silent attacks)
    [SerializeField] private bool muteDuringSlide = true;    // optional (if you can expose slide state)

    [Header("Volumes")]
    [Range(0f, 1f)][SerializeField] private float walkVolume = 1f;
    [Range(0f, 1f)][SerializeField] private float runVolume = 1f;

    private void Awake()
    {
        if (player == null) player = GetComponent<Player>();
        if (rb == null) rb = player != null ? player.rb : GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        SafeStartLoop(walkLoop);
        SafeStartLoop(runLoop);

        SetVolImmediate(walkLoop, 0f);
        SetVolImmediate(runLoop, 0f);
    }

    private void Update()
    {
        if (player == null || rb == null) return;

        // Basic movement checks
        float speedX = Mathf.Abs(rb.linearVelocity.x);
        bool moving = speedX >= minMoveSpeed;

        if (player.currentState == player.moveState)
        {
            moving = true;
        }

        else if (player.currentState == player.crouchState && speedX >= minMoveSpeed)
        {
            moving = true;
        }

        else
        {
            moving = false;
        }

            bool groundedOk = !onlyWhenGrounded || player.isGrounded;

        // Optional: silence during attack / slide if you want
        bool blocked = false;

        if (muteDuringAttacks)
        {
            // You have these booleans already; they’re “pressed this frame”, not “in attack state”.
            // If you want *true attack state*, use animator bools or a flag set by attack states.
            // For now, we’ll just not block on pressed keys (commented).
            blocked |= player.attackOnePressed || player.attackTwoPressed || player.attackThreePressed;
        }

        if (muteDuringSlide)
        {
            // If you later expose a public player.IsSliding flag, you can use it here.
            blocked |= player.currentState == player.slideState;
        }

        bool canPlay = moving && groundedOk && !blocked;

        bool run = false;
        if (useSprintFlag)
        {
            run = player.sprintPressed; // your sprint input flag
        }
        else
        {
            run = speedX > walkMaxSpeed;
        }

        float walkTarget = (canPlay && !run) ? walkVolume : 0f;
        float runTarget = (canPlay && run) ? runVolume : 0f;

        FadeTowards(walkLoop, walkTarget);
        FadeTowards(runLoop, runTarget);
    }

    private void SafeStartLoop(AudioSource src)
    {
        if (src == null) return;
        src.loop = true;
        if (!src.isPlaying) src.Play();
    }

    private void FadeTowards(AudioSource src, float target)
    {
        if (src == null) return;

        // Ensure audible sources are unmuted and playing
        if (target > 0.0001f)
        {
            if (!src.isPlaying) src.Play();
            src.mute = false;
        }

        src.volume = Mathf.MoveTowards(src.volume, target, fadeSpeed * Time.deltaTime);

        // Mute near zero (prevents tiny hiss)
        if (src.volume <= 0.0001f && target <= 0.0001f)
            src.mute = true;
    }

    private void SetVolImmediate(AudioSource src, float v)
    {
        if (src == null) return;
        src.volume = v;
        src.mute = v <= 0.0001f;
    }
}
