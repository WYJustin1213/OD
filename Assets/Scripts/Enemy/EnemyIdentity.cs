using UnityEngine;

public enum EnemySizeType { Small, Large }

public class EnemyIdentity : MonoBehaviour
{
    [SerializeField] private EnemySizeType sizeType = EnemySizeType.Small;

    [Header("Tuning per type")]
    [SerializeField] private float smallMass = 1f;
    [SerializeField] private float largeMass = 3f;

    [SerializeField] private int smallSortingOrder = 0;
    [SerializeField] private int largeSortingOrder = 1;

    [Header("Refs")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer mainRenderer;

    public EnemySizeType SizeType => sizeType;

    private void Reset()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        ApplyTypeSettings();
    }

    public void SetSizeType(EnemySizeType newType)
    {
        sizeType = newType;
        ApplyTypeSettings();
    }

    public void SetMainRenderer(SpriteRenderer sr)
    {
        mainRenderer = sr;
        ApplyTypeSettings();
    }

    private void ApplyTypeSettings()
    {
        if (rb != null)
            rb.mass = (sizeType == EnemySizeType.Small) ? smallMass : largeMass;

        if (mainRenderer != null)
            mainRenderer.sortingOrder = (sizeType == EnemySizeType.Small) ? smallSortingOrder : largeSortingOrder;

        // Layer: make sure these layers exist in Unity and your Physics2D matrix is configured
        int layer = LayerMask.NameToLayer(sizeType == EnemySizeType.Small ? "EnemySmall" : "EnemyLarge");
        if (layer != -1)
            gameObject.layer = layer;
    }
}
