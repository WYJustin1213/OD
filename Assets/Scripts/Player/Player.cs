using Unity.IO.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public PlayerState currentState;

    public PlayerIdle idleState;
    public PlayerJump jumpState;
    public PlayerMove moveState;
    public PlayerCrouch crouchState;
    public PlayerSlide slideState;
    public PlayerAttackOne attackOneState;

    [Header("Core Components")]
    public Combat combat;


    [Header("Components")]
    public Rigidbody2D rb;
    public PlayerInput input;
    public Animator animator;
    public CapsuleCollider2D playerCollider;

    [Header("Movement")]
    public float runSpd;
    public float sprintSpd;
    public float jumpForce;
    public bool jumpPressed;

    public float normalG;
    public float jumpG;
    public float fallG;

    public int faceDir = 1;

    // Input
    public Vector2 moveInput;
    public bool sprintPressed;
    public bool JumpPressed;
    public bool attackOnePressed;
    public bool portalPressed;

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

    public float slideSpd;
    public float slideHeight;
    public float slideWidth;
    public float normalHeight;
    public float normalWidth;
    public Vector2 slideOffset;
    public Vector2 normalOffset;

    private void Awake()
    {
        idleState = new PlayerIdle(this);
        jumpState = new PlayerJump(this);
        moveState = new PlayerMove(this);
        crouchState = new PlayerCrouch(this);
        slideState = new PlayerSlide(this);
        attackOneState = new PlayerAttackOne(this);
    }

    private void Start()
    {
        rb.gravityScale = normalG;

        ChangeState(idleState);
    }

    private void Update()
    {
        currentState.Update();

        if (!isSliding && !attackOnePressed) { Flip(); }
        Animation();
    }

    private void FixedUpdate()
    {
        currentState.FixedUpdate();

        checkGrounded();
    }

    public void ChangeState(PlayerState newState)
    {
        if(currentState != null) { currentState.Exit();  }
        
        currentState = newState;
        currentState.Enter();
    }

    

    public void SetColliderNormal()
    {
        playerCollider.size = new Vector2(normalWidth, normalHeight);
        playerCollider.offset = normalOffset;
    }

    public void SetColliderSlide()
    {
        playerCollider.size = new Vector2(slideWidth, slideHeight);
        playerCollider.offset = slideOffset;
    }


    // changing gravity
    public void ApplyVariableGravity()
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

  

    void checkGrounded()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, gcRadius, groundLayer);

        if (!isGrounded) { isSliding = false; }
    }

    public bool CheckForCeiling()
    {
        return Physics2D.OverlapCircle(headCheck.position, hcRadius, groundLayer);
    }

    // to flip left and right when moving
    void Flip()
    {
        if (!attackOnePressed)
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
    }

    void Animation()
    {
        bool isCrouching = animator.GetBool("isCrouching");
        bool isCrouchWalking = animator.GetBool("isCrouchWalking");

        animator.SetBool("isGrounded", isGrounded);

        animator.SetBool("isFalling", rb.linearVelocity.y < 0.01f && !isGrounded);

        animator.SetFloat("yVel", rb.linearVelocity.y);

        bool isMoving = Mathf.Abs(moveInput.x) > 0.01f && isGrounded;
        
        animator.SetBool("isCrouchWalking", isMoving && isCrouching && !isSliding);

    }


    public void AttackAnimationFished()
    {
        currentState.AttackAnimationFished();
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

    public void OnAttack(InputValue value)
    {
        attackOnePressed = value.isPressed;
    }

    public void OnPortal(InputValue value)
    {
        portalPressed = value.isPressed;
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed && moveInput.y > -0.01f)
        {
            jumpPressed = true;
        }

        else
        {
            jumpPressed = false;
        }
    }
}
