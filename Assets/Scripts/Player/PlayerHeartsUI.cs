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
    [SerializeField] private RectTransform container;

    private readonly List<Image> _hearts = new();
    private bool _subscribed;

    private void Awake()
    {
        if (container == null) container = transform as RectTransform;
    }

    private void OnEnable()
    {
        EnsureRefs();
        Subscribe();
        ForceRefresh();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void EnsureRefs()
    {
        if (playerHealth == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerHealth = p.GetComponentInChildren<Health>();
        }

        if (container == null) container = transform as RectTransform;
    }

    private void Subscribe()
    {
        if (_subscribed) return;
        if (playerHealth == null) return;

        playerHealth.OnHealthChanged += HandleHealthChanged;
        _subscribed = true;
    }

    private void Unsubscribe()
    {
        if (!_subscribed) return;
        if (playerHealth != null) playerHealth.OnHealthChanged -= HandleHealthChanged;
        _subscribed = false;
    }

    private void ForceRefresh()
    {
        // Debug checks (these are the usual reasons nothing appears)
        if (playerHealth == null)
        {
            Debug.LogError("[PlayerHeartsUI] playerHealth is NULL. Assign it or tag your player 'Player' and add Health.", this);
            return;
        }
        if (heartPrefab == null)
        {
            Debug.LogError("[PlayerHeartsUI] heartPrefab is NULL. Drag a UI Image prefab into the field.", this);
            return;
        }
        if (container == null)
        {
            Debug.LogError("[PlayerHeartsUI] container is NULL. Assign a RectTransform container under a Canvas.", this);
            return;
        }
        if (heartFull == null || heartEmpty == null)
        {
            Debug.LogError("[PlayerHeartsUI] heart sprites missing (full/empty). Assign them in Inspector.", this);
            return;
        }

        BuildHearts(playerHealth.maxHealth);
        HandleHealthChanged(playerHealth.health, playerHealth.maxHealth);
    }

    private void BuildHearts(int max)
    {
        for (int i = 0; i < _hearts.Count; i++)
            if (_hearts[i] != null) Destroy(_hearts[i].gameObject);

        _hearts.Clear();

        for (int i = 0; i < max; i++)
        {
            Image heart = Instantiate(heartPrefab, container);
            heart.gameObject.name = $"Heart_{i}";
            heart.sprite = heartFull;
            _hearts.Add(heart);
        }

        //Debug.Log($"[PlayerHeartsUI] Built {max} hearts under '{container.name}'.", this);
    }

    private void HandleHealthChanged(int current, int max)
    {
        if (_hearts.Count != max)
            BuildHearts(max);

        for (int i = 0; i < _hearts.Count; i++)
            _hearts[i].sprite = (i < current) ? heartFull : heartEmpty;
    }
}
