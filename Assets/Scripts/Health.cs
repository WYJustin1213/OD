using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    public event Action OnDamaged;
    public event Action OnDeath;
    public event Action<int, int> OnHealthChanged; // current, max

    public int health;
    public int maxHealth;

    private void Start()
    {
        health = maxHealth;
        OnHealthChanged?.Invoke(health, maxHealth);
    }

    public void ChangeHealth(int amount)
    {
        int old = health;

        health += amount;

        if (health > maxHealth) health = maxHealth;

        if (health <= 0)
        {
            health = 0;
            OnHealthChanged?.Invoke(health, maxHealth);
            OnDeath?.Invoke();
            return;
        }

        if (amount < 0) OnDamaged?.Invoke();

        if (health != old)
            OnHealthChanged?.Invoke(health, maxHealth);
    }
}
