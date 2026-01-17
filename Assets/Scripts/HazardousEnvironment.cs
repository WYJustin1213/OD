using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
public class HazardousEnvironment : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private int damage = 1;
    [SerializeField] private bool damageOverTime = true;
    [SerializeField] private float damageInterval = 0.5f;

    [Header("Knockback (optional)")]
    [SerializeField] private bool applyKnockback = false;
    [SerializeField] private float knockbackForce = 6f;

    [Header("Targets")]
    [SerializeField] private LayerMask targetLayers;

    // Track cooldown per target so standing inside lava doesn't spam damage
    private Dictionary<Health, float> _nextDamageTime = new();

    private void Reset()
    {
        // Hazards should almost always be triggers
        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryDamage(other);
        Debug.Log("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!damageOverTime) return;
        TryDamage(other);
    }

    private void TryDamage(Collider2D other)
    {
        if ((targetLayers.value & (1 << other.gameObject.layer)) == 0)
            return;

        Health health =
            other.GetComponent<Health>() ??
            other.GetComponentInParent<Health>();

        if (health == null) return;

        float time = Time.time;

        if (_nextDamageTime.TryGetValue(health, out float nextTime) && time < nextTime)
            return;

        _nextDamageTime[health] = time + damageInterval;

        // Prefer Hurtbox if present (supports stun / knockback logic)
        Hurtbox hurtbox =
            other.GetComponent<Hurtbox>() ??
            other.GetComponentInParent<Hurtbox>();

        if (hurtbox != null)
        {
            hurtbox.TakeHit(damage, transform);
        }
        else
        {
            health.ChangeHealth(-damage);

            if (applyKnockback)
                ApplyKnockback(other.transform);
        }
    }

    private void ApplyKnockback(Transform target)
    {
        Rigidbody2D rb = target.GetComponent<Rigidbody2D>();
        if (rb == null) return;

        Vector2 dir = (target.position - transform.position).normalized;
        rb.AddForce(dir * knockbackForce, ForceMode2D.Impulse);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Health h =
            other.GetComponent<Health>() ??
            other.GetComponentInParent<Health>();

        if (h != null)
            _nextDamageTime.Remove(h);
    }
}
