using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHeartsUI : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Health targetHealth;

    [Header("UI")]
    [SerializeField] private RectTransform container;
    [SerializeField] private GridLayoutGroup grid;
    [SerializeField] private Image heartPrefab;
    [SerializeField] private Sprite heartFull;
    [SerializeField] private Sprite heartEmpty;

    [Header("World Follow")]
    [SerializeField] private Transform followTarget;                 // optional, defaults to Health transform
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1.2f, 0f);

    [Header("Grid Settings")]
    [SerializeField] private int heartsPerRow = 6;                   // line limit
    [SerializeField] private Vector2 cellSize = new Vector2(0.16f, 0.16f); // world-space UI units (tweak!)
    [SerializeField] private Vector2 cellSpacing = new Vector2(0.02f, 0.02f);

    [Header("Per-heart Bob")]
    [SerializeField] private float heartBobAmplitude = 0.02f;
    [SerializeField] private float heartBobSpeed = 2.0f;

    private readonly List<Image> _hearts = new();

    private void Awake()
    {
        if (container == null) container = transform as RectTransform;
        if (grid == null) grid = GetComponentInChildren<GridLayoutGroup>();

        if (grid != null)
        {
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = Mathf.Max(1, heartsPerRow);
            grid.cellSize = cellSize;
            grid.spacing = cellSpacing;

            // VERY IMPORTANT: don’t let layout expand weirdly
            grid.childAlignment = TextAnchor.UpperCenter;
        }
    }

    private void Start()
    {
        // If this script is on the UI root that is parented under the enemy:
        transform.localPosition = worldOffset;
    }


    private void OnEnable()
    {
        EnsureRefs();

        if (targetHealth != null)
        {
            targetHealth.OnHealthChanged += OnHealthChanged;
            BuildHearts(targetHealth.maxHealth);
            OnHealthChanged(targetHealth.health, targetHealth.maxHealth);
        }
    }

    private void OnDisable()
    {
        if (targetHealth != null)
            targetHealth.OnHealthChanged -= OnHealthChanged;
    }

    /*
    private void LateUpdate()
    {
        if (followTarget == null) return;
        transform.position = followTarget.position;   // no added offset if pivot already has it
        transform.rotation = Quaternion.identity;     // optional: keep upright
    }
    */

    private void EnsureRefs()
    {
        if (targetHealth == null)
            targetHealth = GetComponentInParent<Health>();

        if (followTarget == null && targetHealth != null)
            followTarget = targetHealth.transform;

        if (grid == null)
            grid = GetComponentInChildren<GridLayoutGroup>();

        if (container == null)
            container = (grid != null) ? grid.GetComponent<RectTransform>() : (RectTransform)transform;
    }

    private void BuildHearts(int max)
    {
        for (int i = 0; i < _hearts.Count; i++)
            if (_hearts[i] != null) Destroy(_hearts[i].transform.parent.gameObject); // destroy wrapper

        _hearts.Clear();

        if (grid != null)
            grid.constraintCount = Mathf.Max(1, heartsPerRow);

        for (int i = 0; i < max; i++)
        {
            // 1) Wrapper (this is what GridLayoutGroup positions)
            GameObject wrapper = new GameObject($"EnemyHeartWrap_{i}", typeof(RectTransform));
            RectTransform wrapRT = wrapper.GetComponent<RectTransform>();
            wrapRT.SetParent(container, worldPositionStays: false);
            wrapRT.localScale = Vector3.one;

            // Optional: force wrapper size to match grid cell size (helps some setups)
            wrapRT.sizeDelta = (grid != null) ? grid.cellSize : cellSize;

            // 2) Actual heart image inside wrapper (this is what bobs)
            Image heart = Instantiate(heartPrefab, wrapRT);
            heart.gameObject.name = $"EnemyHeart_{i}";

            RectTransform heartRT = heart.rectTransform;
            heartRT.anchorMin = new Vector2(0.5f, 0.5f);
            heartRT.anchorMax = new Vector2(0.5f, 0.5f);
            heartRT.pivot = new Vector2(0.5f, 0.5f);
            heartRT.anchoredPosition = Vector2.zero;
            heartRT.localScale = Vector3.one;

            heart.sprite = heartFull;

            // Bob ONLY the child image, not the wrapper
            HeartBob bob = heart.GetComponent<HeartBob>();
            if (bob == null) bob = heart.gameObject.AddComponent<HeartBob>();
            bob.Configure(heartBobAmplitude, heartBobSpeed);

            _hearts.Add(heart);
        }
    }


    private void OnHealthChanged(int current, int max)
    {
        if (_hearts.Count != max)
            BuildHearts(max);

        for (int i = 0; i < _hearts.Count; i++)
            _hearts[i].sprite = (i < current) ? heartFull : heartEmpty;
    }

    // Optional: change at runtime (universe effects etc.)
    public void SetHeartsPerRow(int perRow)
    {
        heartsPerRow = Mathf.Max(1, perRow);
        if (grid != null) grid.constraintCount = heartsPerRow;
    }
}
