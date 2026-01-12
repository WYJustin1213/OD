using UnityEngine;

public enum BlockReason { None, Wall, Cliff, SameTypeEnemy }

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyMotor2D : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private EnemyIdentity identity;

    [Header("Check Origins")]
    [SerializeField] private Transform groundCheckOrigin;
    [SerializeField] private Transform wallCheckOrigin;

    [Header("Distances")]
    [SerializeField] private float groundCheckAhead = 0.4f;
    [SerializeField] private float groundCheckDown = 0.9f;
    [SerializeField] private float wallCheckDistance = 0.25f;
    [SerializeField] private float enemyCheckDistance = 0.35f;

    [Header("Masks")]
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private LayerMask enemyMask;

    private float _facing = 1f;
    public float Facing => _facing;

    private bool _facingLocked;
    public bool IsFacingLocked => _facingLocked;

    private bool _movementLocked;
    public bool IsMovementLocked => _movementLocked;

    // hard lock (stops animation/transform movement too)
    private float _lockedWorldX;

    private void Reset()
    {
        rb = GetComponent<Rigidbody2D>();
        identity = GetComponent<EnemyIdentity>();
    }

    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (identity == null) identity = GetComponent<EnemyIdentity>();
    }

    private void FixedUpdate()
    {
        if (!_movementLocked) return;

        // Kill velocity-based movement
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    private void LateUpdate()
    {
        if (!_movementLocked) return;

        // Kill transform/animation-based movement
        Vector3 p = transform.position;
        if (Mathf.Abs(p.x - _lockedWorldX) > 0.0001f)
            transform.position = new Vector3(_lockedWorldX, p.y, p.z);
    }

    public void SetMovementLocked(bool locked)
    {
        _movementLocked = locked;

        if (locked)
        {
            _lockedWorldX = transform.position.x;
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }
    }

    public void SetFacingLocked(bool locked)
    {
        _facingLocked = locked;
    }

    public void SetFacing(float dir)
    {
        if (_facingLocked) return;                // <-- new
        
        if (Mathf.Abs(dir) < 0.001f) return;
        _facing = Mathf.Sign(dir);

        // flip sprite by scale
        Vector3 s = transform.localScale;
        s.x = Mathf.Abs(s.x) * _facing;
        transform.localScale = s;
    }

    public BlockReason GetForwardBlockReason()
    {
        Vector2 wallOrigin = wallCheckOrigin ? (Vector2)wallCheckOrigin.position : rb.position;
        Vector2 groundOrigin = groundCheckOrigin ? (Vector2)groundCheckOrigin.position : rb.position;

        // Same-type enemy check
        RaycastHit2D enemyHit = Physics2D.Raycast(wallOrigin, Vector2.right * _facing, enemyCheckDistance, enemyMask);
        if (enemyHit.collider != null)
        {
            EnemyIdentity other = enemyHit.collider.GetComponentInParent<EnemyIdentity>();
            if (other != null && identity != null && other.SizeType == identity.SizeType)
                return BlockReason.SameTypeEnemy;
        }

        // Wall check
        RaycastHit2D wallHit = Physics2D.Raycast(wallOrigin, Vector2.right * _facing, wallCheckDistance, groundMask);
        if (wallHit.collider != null)
            return BlockReason.Wall;

        // Cliff check
        Vector2 aheadPoint = groundOrigin + Vector2.right * _facing * groundCheckAhead;
        RaycastHit2D groundHit = Physics2D.Raycast(aheadPoint, Vector2.down, groundCheckDown, groundMask);
        if (groundHit.collider == null)
            return BlockReason.Cliff;

        return BlockReason.None;
    }

    public void MoveHorizontally(float desiredSpeed, bool respectBlocks = true)
    {
        if (_movementLocked)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        if (Mathf.Abs(desiredSpeed) > 0.01f)
            SetFacing(desiredSpeed);

        if (respectBlocks && Mathf.Abs(desiredSpeed) > 0.01f && GetForwardBlockReason() != BlockReason.None)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        rb.linearVelocity = new Vector2(desiredSpeed, rb.linearVelocity.y);
    }

    public void SetGroundCheckOrigin(Transform t) => groundCheckOrigin = t;
    public void SetWallCheckOrigin(Transform t) => wallCheckOrigin = t;
}
