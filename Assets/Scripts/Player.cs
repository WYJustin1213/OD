using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("Components")]
    public Rigidbody2D rb;
    public PlayerInput input;
    public Animator animator;
    public CapsuleCollider2D playerCollider;

    [Header("Movement")]
    public float runSpd;
    public float sprintSpd;
    public float jumpForce;

    public float normalG;
    public float jumpG;
    public float fallG;

    public int faceDir = 1;

    // Input
    public Vector2 moveInput;
    private bool sprintPressed;
    private bool JumpPressed;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float gcRadius;
    public LayerMask groundLayer;
    public bool isGrounded;

    [Header("Crouch")]
    public Transform headCheck;
    public float hcRadius;
    public float crouchSpd;

    [Header("Slide")]
    public float slideDuration = 0.6f;
    public float slideStopDur = 0.15f;

    private bool isSliding;
    private bool slideLock;
    private float slideTimer;
    private float slideStopTimer;

    public float slideSpd;
    public float slideHeight;
    public float slideWidth;
    public float normalHeight;
    public float normalWidth;
    public Vector2 slideOffset;
    public Vector2 normalOffset;


    private void Start()
    {
        rb.gravityScale = normalG;
    }

    private void Update()
    {
        if (!isSliding) { Flip(); }
        Animation();
        Slide();
        TryStandUp();
    }

    private void FixedUpdate()
    {
        if (!isSliding) 
        { 
            Movement(); 
            Jump(); 
        }

        ApplyVariableGravity();
        checkGrounded();
    }

    private void Movement()
    {
        bool isCrouching = animator.GetBool("isCrouching");
        bool isCrouchWalking = animator.GetBool("isCrouchWalking");

        if (!isCrouching && !isCrouchWalking && !isSliding)
        {
            float currentSpd = sprintPressed ? sprintSpd : runSpd;
            float targetSpeed = moveInput.x * currentSpd;
            rb.linearVelocity = new Vector2(targetSpeed, rb.linearVelocity.y);
        }

        else
        {
            float targetSpeed = moveInput.x * runSpd;
            rb.linearVelocity = new Vector2(targetSpeed, rb.linearVelocity.y);
        }
    }

    private void Jump()
    {
        if (JumpPressed && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            JumpPressed = false;
        }
    }

    private void Slide()
    {
        if (isSliding)
        {
            slideTimer -= Time.deltaTime;
            rb.linearVelocity = new Vector2 (slideSpd * faceDir, rb.linearVelocity.y);

            // stop sliding
            if (slideTimer <= 0)
            {
                isSliding = false;
                slideLock = false;
                slideStopTimer = slideStopDur;
                TryStandUp();
            }
        }

        
        if (slideStopTimer > 0)
        {
            slideStopTimer -= Time.deltaTime;
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            isSliding = false;
        }

        if (slideStopTimer < 0 && moveInput.y >= -0.01f)
        {
            slideLock = false;
        }

        bool isCrouching = animator.GetBool("isCrouching");
        bool isCrouchWalking = animator.GetBool("isCrouchWalking");

        // start sliding
        if (isGrounded && !isCrouching && sprintPressed && moveInput.y < -0.01f && !isSliding && !slideLock && !isCrouchWalking)
        {
            isSliding = true;
            slideLock = true;
            slideTimer = slideDuration;
            SetColliderSlide();
        }
    }

    void SetColliderNormal()
    {
        playerCollider.size = new Vector2(normalWidth, normalHeight);
        playerCollider.offset = normalOffset;
    }

    void SetColliderSlide()
    {
        playerCollider.size = new Vector2(slideWidth, slideHeight);
        playerCollider.offset = slideOffset;
    }


    // changing gravity
    void ApplyVariableGravity()
    {
        if (rb.linearVelocity.y < -0.1f)
        {
            rb.gravityScale = fallG;
        }
        else if (rb.linearVelocity.y > 0.1f)
        {
            rb.gravityScale = jumpG;
        }
        else
        {
            rb.gravityScale = normalG;
        }
    }

    void TryStandUp()
    {
        if (isSliding)
        {
            animator.SetBool("isCrouching", false);
            return;
        }

        bool isMoving = Mathf.Abs(moveInput.x) > 0.01f && isGrounded;
        bool shouldCrouch = moveInput.y <= -0.1f;

        if (!shouldCrouch)
        {
            SetColliderNormal();
            animator.SetBool("isCrouching", false);
        }
        else 
        {
            SetColliderSlide();
            animator.SetBool("isCrouching", true);
        }
    }

    void checkGrounded()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, gcRadius, groundLayer);

        if (!isGrounded) { isSliding = false; }
    }

    // to flip left and right when moving
    void Flip()
    {
        if (moveInput.x > 0.1f)
        {
            faceDir = 1;
        }
        else if (moveInput.x < -0.1f)
        {
            faceDir = -1;
        }

        transform.localScale = new Vector3(faceDir, 1, 1);
    }

    void Animation()
    {
        bool isCrouching = animator.GetBool("isCrouching");
        bool isCrouchWalking = animator.GetBool("isCrouchWalking");

        animator.SetBool("isGrounded", isGrounded);

        animator.SetBool("isJumping", rb.linearVelocity.y > 0.01f && !isSliding && !isCrouching && !isCrouchWalking);
        animator.SetBool("isFalling", rb.linearVelocity.y < 0.01f && !isGrounded);

        animator.SetFloat("yVel", rb.linearVelocity.y);

        bool isMoving = Mathf.Abs(moveInput.x) > 0.01f && isGrounded;

        animator.SetBool("isIdle", !isMoving && isGrounded && !isSliding && !isCrouching && !isCrouchWalking);
        animator.SetBool("isRunning", isMoving && !sprintPressed && !isSliding && !isCrouching && !isCrouchWalking);
        animator.SetBool("isCrouchWalking", isMoving && isCrouching && !isSliding);
        animator.SetBool("isSprinting", isMoving && sprintPressed && !isSliding && !isCrouching && !isCrouchWalking);

        animator.SetBool("isSliding", isSliding);
    }



    // system
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnSprint(InputValue value)
    {
        sprintPressed = value.isPressed;
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed && moveInput.y > -0.1f)
        {
            JumpPressed = true;
        }

        else
        {
            JumpPressed = false;
        }
    }


    private void onDrawGizmoSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, gcRadius);

        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(headCheck.position, hcRadius);

    }
}
