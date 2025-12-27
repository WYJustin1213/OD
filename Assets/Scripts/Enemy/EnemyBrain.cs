using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyBrain : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player; // assign or find at runtime
    [SerializeField] private Rigidbody2D rb;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3.0f;

    [Header("Perception")]
    [SerializeField] private float allyRadius = 6f;
    [SerializeField] private LayerMask allyLayer;

    private IAIPolicy _policy;

    private void Reset()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        if (UniverseManager.Instance != null)
            UniverseManager.Instance.UniverseChanged += HandleUniverseChanged;

        // Initialize policy based on current universe
        if (UniverseManager.Instance != null)
            ApplyPolicyForUniverse(UniverseManager.Instance.CurrentUniverse);
        else
            _policy = new GatherPolicy(); // safe default
    }

    private void OnDisable()
    {
        if (UniverseManager.Instance != null)
            UniverseManager.Instance.UniverseChanged -= HandleUniverseChanged;
    }

    private void HandleUniverseChanged(UniverseId oldU, UniverseId newU)
    {
        ApplyPolicyForUniverse(newU);
    }

    private void ApplyPolicyForUniverse(UniverseId universe)
    {
        // You can replace this with UniverseDefinition data later.
        _policy = universe switch
        {
            UniverseId.U1 => new GatherPolicy(),
            UniverseId.U2 => new ScatterPolicy(),
            _ => new GatherPolicy()
        };
    }

    private void FixedUpdate()
    {
        if (_policy == null) return;
        if (player == null) return;

        // Build context
        AIContext ctx = BuildContext();

        // Decide
        AIIntent intent = _policy.Decide(in ctx);

        // Execute (simple locomotion)
        rb.linearVelocity = intent.desiredVelocity;

        // Attack is just a flag here; wire into attack module later.
        // if (intent.wantsAttack) Attack();
    }

    private AIContext BuildContext()
    {
        Vector2 selfPos = rb.position;
        Vector2 playerPos = (Vector2)player.position; 

        // Compute allies centroid
        Vector2 centroid = Vector2.zero;
        int count = 0;

        Collider2D[] hits = Physics2D.OverlapCircleAll(selfPos, allyRadius, allyLayer);
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].attachedRigidbody == rb) continue; // skip self
            centroid += (Vector2)hits[i].transform.position;
            count++;
        }

        if (count > 0) centroid /= count;
        else centroid = selfPos;

        return new AIContext
        {
            self = transform,
            player = player,
            selfPos = selfPos,
            playerPos = playerPos,
            moveSpeed = moveSpeed,
            alliesCentroid = centroid,
            allyCount = count,
            time = Time.time,
            deltaTime = Time.fixedDeltaTime
        };
    }
}
