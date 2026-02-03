using System;
using UnityEngine;

public class Stamina : MonoBehaviour
{
    public event Action<int, int> OnStaminaChanged;   // current, max
    public event Action OnStaminaFailed;              // not enough

    [Header("Stamina")]
    [SerializeField] private int maxStamina = 5;
    [SerializeField] private int stamina;

    [Header("Regen")]
    [SerializeField] private int regenAmount = 1;
    [SerializeField] private float regenInterval = 1f;

    private float _nextRegenTime;

    public int Current => stamina;
    public int Max => maxStamina;

    private void Start()
    {
        stamina = maxStamina;
        _nextRegenTime = Time.time + regenInterval;
        OnStaminaChanged?.Invoke(stamina, maxStamina);
    }

    private void Update()
    {
        if (stamina >= maxStamina) return;

        if (Time.time >= _nextRegenTime)
        {
            stamina = Mathf.Min(maxStamina, stamina + regenAmount);
            _nextRegenTime = Time.time + regenInterval;
            OnStaminaChanged?.Invoke(stamina, maxStamina);
        }
    }

    public bool TrySpend(int cost)
    {
        cost = Mathf.Max(0, cost);

        if (stamina < cost)
        {
            OnStaminaFailed?.Invoke();
            return false;
        }

        stamina -= cost;
        OnStaminaChanged?.Invoke(stamina, maxStamina);

        // Optional: push regen back slightly when spending
        _nextRegenTime = Mathf.Max(_nextRegenTime, Time.time + 0.1f);

        return true;
    }

    // Optional future helpers
    public void SetMax(int newMax, bool fill = true)
    {
        maxStamina = Mathf.Max(1, newMax);
        stamina = fill ? maxStamina : Mathf.Min(stamina, maxStamina);
        OnStaminaChanged?.Invoke(stamina, maxStamina);
    }
}
