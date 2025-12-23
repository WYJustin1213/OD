using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("Components")]
    public Rigidbody2D rb;
    public PlayerInput input;
    public Animator animator;

    [Header("Movement")]
    public float speed;
    public float jumpForce;
    //public float PowerJumpForce;

    public float normalG;
    public float jumpG;
    public float fallG;

    public int faceDir = 1;

    // Input
    public Vector2 moveInput;

    private bool JumpPressed;
    //private bool JumpReleased;

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
        float targetSpeed = moveInput.x * speed;
        rb.linearVelocity = new Vector2(targetSpeed, rb.linearVelocity.y);
    }

    private void Jump()
    {
        if (JumpPressed && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            JumpPressed = false;
            //JumpReleased = false;
        }

        /*
        if (JumpReleased) 
        {
            if (rb.linearVelocity.y > 0)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, PowerJumpForce);
                JumpReleased = false;
            }
        }
        */
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
        animator.SetBool("isJumping", rb.linearVelocity.y > 0.1f);
        animator.SetBool("isGrounded", isGrounded);

        animator.SetFloat("yVel", rb.linearVelocity.y);

        animator.SetBool("isIdle", Mathf.Abs(moveInput.x) < 0.1f && isGrounded);
        animator.SetBool("isRunning", Mathf.Abs(moveInput.x) > 0.1f && isGrounded);
    }



    // system
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed)
        {
            JumpPressed = true;
            //JumpReleased = false;
        }

        /*
        else
        {
            JumpReleased = true;
        }
        */
    }


    private void onDrawGizmoSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, gcRadius);
    }
}
