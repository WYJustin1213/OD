using UnityEngine;

public enum BlockReason
{
    None,
    Wall,
    Cliff,
    SameTypeEnemy
}


[RequireComponent(typeof(Rigidbody2D))]
public class EnemyMotor2D : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private EnemyIdentity identity;

    [Header("Check Origins")]
    [SerializeField] private Transform groundCheckOrigin; // near feet
    [SerializeField] private Transform wallCheckOrigin;   // near chest/front

    [Header("Distances")]
    [SerializeField] private float groundCheckAhead = 0.4f;
    [SerializeField] private float groundCheckDown = 0.9f;
    [SerializeField] private float wallCheckDistance = 0.25f;
    [SerializeField] private float enemyCheckDistance = 0.35f;

    [Header("Masks")]
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private LayerMask enemyMask; // should include both EnemySmall and EnemyLarge layers

    private float _facing = 1f;

    public float Facing => _facing;

    private bool _movementLocked;

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

    private void Update()
    {
        
    }

    public void SetMovementLocked(bool locked)
    {
        _movementLocked = locked;
        if (locked && rb != null)
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    public void SetFacing(float dir)
    {
        if (Mathf.Abs(dir) < 0.001f) return;
        _facing = Mathf.Sign(dir);

        // Optional flip (only if you want scale flip)
        Vector3 s = transform.localScale;
        s.x = Mathf.Abs(s.x) * _facing;
        transform.localScale = s;
    }

    public BlockReason GetForwardBlockReason()
    {
        Vector2 wallOrigin = wallCheckOrigin ? (Vector2)wallCheckOrigin.position : rb.position;
        Vector2 groundOrigin = groundCheckOrigin ? (Vector2)groundCheckOrigin.position : rb.position;

        // 1) Same-type enemy check (short ray)
        RaycastHit2D enemyHit = Physics2D.Raycast(wallOrigin, Vector2.right * _facing, enemyCheckDistance, enemyMask);
        if (enemyHit.collider != null)
        {
            EnemyIdentity other = enemyHit.collider.GetComponentInParent<EnemyIdentity>();
            if (other != null && identity != null && other.SizeType == identity.SizeType)
                return BlockReason.SameTypeEnemy;
        }

        // 2) Wall check
        RaycastHit2D wallHit = Physics2D.Raycast(wallOrigin, Vector2.right * _facing, wallCheckDistance, groundMask);
        if (wallHit.collider != null)
            return BlockReason.Wall;

        // 3) Cliff check (ground ahead?)
        Vector2 aheadPoint = groundOrigin + Vector2.right * _facing * groundCheckAhead;
        RaycastHit2D groundHit = Physics2D.Raycast(aheadPoint, Vector2.down, groundCheckDown, groundMask);
        if (groundHit.collider == null)
            return BlockReason.Cliff;

        return BlockReason.None;
    }

    public bool CanMoveForward() => GetForwardBlockReason() == BlockReason.None;

    public void MoveHorizontally(float desiredSpeed, bool respectBlocks = true)
    {
        if (_movementLocked)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        if (Mathf.Abs(desiredSpeed) > 0.01f)
            SetFacing(desiredSpeed);

        if (respectBlocks && Mathf.Abs(desiredSpeed) > 0.01f)
        {
            if (GetForwardBlockReason() != BlockReason.None)
            {
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
                return;
            }
        }

        rb.linearVelocity = new Vector2(desiredSpeed, rb.linearVelocity.y);
    }

    // Setters for variant injection if you want
    public void SetGroundCheckOrigin(Transform t) => groundCheckOrigin = t;
    public void SetWallCheckOrigin(Transform t) => wallCheckOrigin = t;

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        float facing = Application.isPlaying ? _facing : 1f;
        Vector2 wallOrigin = wallCheckOrigin ? (Vector2)wallCheckOrigin.position : (Vector2)transform.position;
        Vector2 groundOrigin = groundCheckOrigin ? (Vector2)groundCheckOrigin.position : (Vector2)transform.position;
        Vector2 aheadPoint = groundOrigin + Vector2.right * facing * groundCheckAhead;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(wallOrigin, wallOrigin + Vector2.right * facing * wallCheckDistance);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(aheadPoint, aheadPoint + Vector2.down * groundCheckDown);
    }
#endif
}
