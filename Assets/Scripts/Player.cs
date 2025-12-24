using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("Components")]
    public Rigidbody2D rb;
    public PlayerInput input;
    public Animator animator;

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

    private void Start()
    {
        rb.gravityScale = normalG;
    }

    private void Update()
    {
        Flip();
        Animation();
    }

    private void FixedUpdate()
    {
        Movement();

        ApplyVariableGravity();
        checkGrounded();
        Jump();
    }

    private void Movement()
    {
        float currentSpd = sprintPressed ? sprintSpd : runSpd;
        float targetSpeed = moveInput.x * currentSpd;
        rb.linearVelocity = new Vector2(targetSpeed, rb.linearVelocity.y);
    }

    private void Jump()
    {
        if (JumpPressed && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            JumpPressed = false;
        }
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

    void checkGrounded()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, gcRadius, groundLayer);
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
        animator.SetBool("isGrounded", isGrounded);

        animator.SetBool("isJumping", rb.linearVelocity.y > 0.01f);
        animator.SetBool("isFalling", rb.linearVelocity.y < 0.01f && !isGrounded);

        animator.SetFloat("yVel", rb.linearVelocity.y);

        bool isMoving = Mathf.Abs(moveInput.x) > 0.01f && isGrounded;

        animator.SetBool("isIdle", !isMoving && isGrounded);
        animator.SetBool("isRunning", isMoving && !sprintPressed);
        animator.SetBool("isSprinting", isMoving && sprintPressed);
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
        if (value.isPressed)
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
    }
}
