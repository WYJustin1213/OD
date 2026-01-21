using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class KnockbackReceiver2D : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;

    [Header("Tuning")]
    [SerializeField] private float verticalBoost = 0.2f;     // small lift so it feels punchy
    [SerializeField] private bool zeroXBeforeKnock = true;

    private void Reset()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
    }

    public void ApplyKnockback(Vector2 direction, float force)
    {
        direction.Normalize();

        if (zeroXBeforeKnock)
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        Vector2 impulse = new Vector2(direction.x, direction.y + verticalBoost) * force;
        rb.AddForce(impulse, ForceMode2D.Impulse);
    }
}
