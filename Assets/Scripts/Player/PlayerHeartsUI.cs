using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHeartsUI : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Health playerHealth;

    [Header("Prefabs & Sprites")]
    [SerializeField] private Image heartPrefab;      // prefab with Image component
    [SerializeField] private Sprite heartFull;
    [SerializeField] private Sprite heartEmpty;

    [Header("Layout")]
    [SerializeField] private RectTransform container; // a UI panel with HorizontalLayoutGroup recommended

    private readonly List<Image> _hearts = new();

    private void Awake()
    {
        if (container == null) container = (RectTransform)transform;
    }

    private void OnEnable()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged += HandleHealthChanged;
    }

    private void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged -= HandleHealthChanged;
    }

    private void Start()
    {
        if (playerHealth == null)
        {
            // Try find player by tag
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerHealth = p.GetComponentInChildren<Health>();
        }

        if (playerHealth != null)
        {
            // Build once with current max
            BuildHearts(playerHealth.maxHealth);
            HandleHealthChanged(playerHealth.health, playerHealth.maxHealth);
        }
    }

    private void BuildHearts(int max)
    {
        // Clear old
        for (int i = 0; i < _hearts.Count; i++)
            if (_hearts[i] != null) Destroy(_hearts[i].gameObject);

        _hearts.Clear();

        for (int i = 0; i < max; i++)
        {
            Image heart = Instantiate(heartPrefab, container);
            heart.sprite = heartFull;
            _hearts.Add(heart);
        }
    }

    private void HandleHealthChanged(int current, int max)
    {
        // If max changed (future upgrades), rebuild
        if (_hearts.Count != max)
            BuildHearts(max);

        for (int i = 0; i < _hearts.Count; i++)
        {
            bool filled = i < current;
            _hearts[i].sprite = filled ? heartFull : heartEmpty;
        }
    }
}
