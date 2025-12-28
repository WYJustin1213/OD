using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyMotor2D : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private Rigidbody2D rb;

    [Header("Collision Checks")]
    [SerializeField] private Transform groundCheckOrigin;  // place near feet center
    [SerializeField] private Transform wallCheckOrigin;    // place near chest/front
    [SerializeField] private float groundCheckAhead = 0.4f;
    [SerializeField] private float groundCheckDown = 0.8f;
    [SerializeField] private float wallCheckDistance = 0.2f;

    [SerializeField] private LayerMask groundMask;

    private float _facing = 1f;

    private void Reset()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void SetFacing(float dir)
    {
        if (Mathf.Abs(dir) < 0.001f) return;
        _facing = Mathf.Sign(dir);

        // Optional sprite flip:
        Vector3 s = transform.localScale;
        s.x = Mathf.Abs(s.x) * _facing;
        transform.localScale = s;
    }

    public bool CanMoveForward()
    {
        // Wall check
        Vector2 wallOrigin = wallCheckOrigin ? (Vector2)wallCheckOrigin.position : rb.position;
        RaycastHit2D wallHit = Physics2D.Raycast(wallOrigin, Vector2.right * _facing, wallCheckDistance, groundMask);
        if (wallHit.collider != null)
            return false;

        // Cliff check (is there ground ahead?)
        Vector2 groundOrigin = groundCheckOrigin ? (Vector2)groundCheckOrigin.position : rb.position;
        Vector2 aheadPoint = groundOrigin + Vector2.right * _facing * groundCheckAhead;

        RaycastHit2D groundHit = Physics2D.Raycast(aheadPoint, Vector2.down, groundCheckDown, groundMask);
        if (groundHit.collider == null)
            return false;

        return true;
    }

    public void MoveHorizontally(float desiredSpeed)
    {
        // If trying to move forward but blocked, stop horizontal velocity
        if (Mathf.Abs(desiredSpeed) > 0.01f)
        {
            SetFacing(desiredSpeed);

            if (!CanMoveForward())
            {
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
                return;
            }
        }

        rb.linearVelocity = new Vector2(desiredSpeed, rb.linearVelocity.y);
    }

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
