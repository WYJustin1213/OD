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
    public PlayerAttackTwo attackTwoState;
    public PlayerAttackThree attackThreeState;
    public PlayerHit hitState;
    public PlayerStepUp stepUpState;
    //public PlayerWallClimb wallClimbState;

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
    public bool attackTwoPressed;
    public bool attackThreePressed;
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
    
    public bool inputLocked;

    [Header("Combat")]
    public int attackOneDamage;
    public int attackTwoDamage;
    public int attackThreeDamage;

    [Header("Step Up (Auto Climb)")]
    public bool enableAutoStep = true;
    public float stepCheckDistance = 0.2f;
    public float stepHeight = 0.5f;        // max ledge height
    public float stepForward = 0.25f;      // how far forward to move
    public float stepDuration = 0.15f;
    public Transform stepCheckOriginLow;   // near feet
    public Transform stepCheckOriginHigh;  // at stepHeight
    public LayerMask stepGroundLayer;

    /*
    [Header("Wall Climb")]
    public bool enableWallClimb = true;
    public Transform wallCheck;            // chest/hand height
    public float wallCheckRadius = 0.15f;
    public LayerMask climbableWallLayer;
    public float wallClimbUp = 1.0f;       // how high to climb (tune to tile height)
    public float wallClimbForward = 0.3f;  // mantle forward
    public float wallClimbDuration = 0.25f;

    [Header("Climb Input")]
    public bool climbPressed;              // W
    public bool climbUpPressed;            // Space (jump)
    */

    private void Awake()
    {
        idleState = new PlayerIdle(this);
        jumpState = new PlayerJump(this);
        moveState = new PlayerMove(this);
        crouchState = new PlayerCrouch(this);
        slideState = new PlayerSlide(this);
        attackOneState = new PlayerAttackOne(this);
        attackTwoState = new PlayerAttackTwo(this);
        attackThreeState = new PlayerAttackThree(this);
        hitState = new PlayerHit(this);
        stepUpState = new PlayerStepUp(this);
        //wallClimbState = new PlayerWallClimb(this);
    }

    private void Start()
    {
        rb.gravityScale = normalG;

        ChangeState(idleState);
    }

    private void Update()
    {
        if (!inputLocked)
        {
            if (!isSliding && !attackOnePressed) { Flip(); }
            AttackOne();
            AttackTwo();
            AttackThree();
            //Climb();
        }

        Animation();
        currentState.Update();
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

    public void TakeHitFromEnemy(Transform attacker)
    {
        // You can ignore hits if dead, or add i-frames checks elsewhere.
        hitState.SetAttacker(attacker);
        ChangeState(hitState);
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

    public bool CanAutoStepUp()
    {
        if (!enableAutoStep) return false;
        if (!isGrounded) return false;
        if (Mathf.Abs(moveInput.x) < 0.01f) return false;
        if (CheckForCeiling()) return false; // optional safety

        Vector2 dir = Vector2.right * faceDir;

        // Low ray: is there a block in front at foot height?
        Vector2 lowOrigin = stepCheckOriginLow ? (Vector2)stepCheckOriginLow.position : (Vector2)transform.position;
        RaycastHit2D lowHit = Physics2D.Raycast(lowOrigin, dir, stepCheckDistance, stepGroundLayer);
        if (!lowHit.collider) return false;

        // High ray: is there free space above the ledge?
        Vector2 highOrigin = stepCheckOriginHigh ? (Vector2)stepCheckOriginHigh.position : lowOrigin + Vector2.up * stepHeight;
        RaycastHit2D highHit = Physics2D.Raycast(highOrigin, dir, stepCheckDistance, stepGroundLayer);
        if (highHit.collider) return false;

        return true;
    }

    /*
    public bool IsTouchingClimbableWall()
    {
        if (!enableWallClimb) return false;
        if (!wallCheck) return false;

        return Physics2D.OverlapCircle(wallCheck.position, wallCheckRadius, climbableWallLayer);
    }
    */

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

    public void AttackOne()
    {
        Debug.Log("1" + attackOnePressed);
        attackOnePressed = Input.GetKeyDown(KeyCode.J);
    }

    public void AttackTwo()
    {
        Debug.Log("2" + attackTwoPressed);
        attackTwoPressed = Input.GetKeyDown(KeyCode.K);
    }

    public void AttackThree()
    {
        Debug.Log("3" + attackThreePressed);
        attackThreePressed = Input.GetKeyDown(KeyCode.L);
    }

    /*
    public void Climb()
    {
        climbPressed = Input.GetKey(KeyCode.W);
    }
    */

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

public enum PlayerAttackType
{
    AttackOne,
    AttackTwo,
    AttackThree
}
