using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStaminaUI : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Stamina stamina;

    [Header("Prefabs & Sprites")]
    [SerializeField] private Image pipPrefab;
    [SerializeField] private Sprite pipFull;
    [SerializeField] private Sprite pipEmpty;

    [Header("Layout")]
    [SerializeField] private RectTransform container;

    [Header("Not enough feedback")]
    [SerializeField] private float popScale = 1.25f;
    [SerializeField] private float popDuration = 1.0f;

    private readonly List<Image> _pips = new();
    private Coroutine _popRoutine;
    private Vector3 _baseScale;

    private void Awake()
    {
        if (container == null) container = (RectTransform)transform;
        _baseScale = container.localScale;
    }

    private void OnEnable()
    {
        EnsureTarget();

        if (stamina != null)
        {
            stamina.OnStaminaChanged += HandleStaminaChanged;
            stamina.OnStaminaFailed += Pop;
        }
    }

    private void OnDisable()
    {
        if (stamina != null)
        {
            stamina.OnStaminaChanged -= HandleStaminaChanged;
            stamina.OnStaminaFailed -= Pop;
        }
    }

    private void Start()
    {
        EnsureTarget();

        if (stamina != null)
        {
            Build(stamina.Max);
            HandleStaminaChanged(stamina.Current, stamina.Max);
        }
    }

    private void EnsureTarget()
    {
        if (stamina != null) return;

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) stamina = p.GetComponentInChildren<Stamina>();
    }

    private void Build(int max)
    {
        for (int i = 0; i < _pips.Count; i++)
            if (_pips[i] != null) Destroy(_pips[i].gameObject);
        _pips.Clear();

        for (int i = 0; i < max; i++)
        {
            Image pip = Instantiate(pipPrefab, container);
            pip.gameObject.name = $"Stamina_{i}";
            pip.sprite = pipFull;
            _pips.Add(pip);
        }
    }

    private void HandleStaminaChanged(int current, int max)
    {
        if (_pips.Count != max)
            Build(max);

        for (int i = 0; i < _pips.Count; i++)
            _pips[i].sprite = (i < current) ? pipFull : pipEmpty;
    }

    private void Pop()
    {
        if (_popRoutine != null) StopCoroutine(_popRoutine);
        _popRoutine = StartCoroutine(PopRoutine());
    }

    private IEnumerator PopRoutine()
    {
        container.localScale = _baseScale * popScale;

        float t = 0f;
        while (t < popDuration)
        {
            t += Time.deltaTime;
            // optional: ease back
            float k = Mathf.Clamp01(t / popDuration);
            container.localScale = Vector3.Lerp(_baseScale * popScale, _baseScale, k);
            yield return null;
        }

        container.localScale = _baseScale;
        _popRoutine = null;
    }
}
