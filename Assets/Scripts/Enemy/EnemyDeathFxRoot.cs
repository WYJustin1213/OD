using UnityEngine;

[RequireComponent(typeof(Health))]
public class EnemyDeathFxRoot : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Health health;

    // Assign in inspector OR let Awake find it
    [SerializeField] private UniverseEnemyVariantSwitcher variantSwitcher;

    private void Awake()
    {
        if (health == null) health = GetComponent<Health>();
        if (variantSwitcher == null) variantSwitcher = GetComponent<UniverseEnemyVariantSwitcher>();
    }

    private void OnEnable()
    {
        if (health != null) health.OnDeath += HandleDeath;
    }

    private void OnDisable()
    {
        if (health != null) health.OnDeath -= HandleDeath;
    }

    private void HandleDeath()
    {
        EnemyDeathFxProfile profile = null;

        // Preferred: ask the switcher which variant is active (we add a helper below)
        if (variantSwitcher != null)
            profile = variantSwitcher.GetActiveDeathFxProfile();

        // Fallback: find any enabled profile in children
        if (profile == null)
            profile = GetComponentInChildren<EnemyDeathFxProfile>(includeInactive: false);

        if (profile != null)
            SpawnDeathParts(profile);

        Destroy(gameObject);
    }

    private void SpawnDeathParts(EnemyDeathFxProfile profile)
    {
        if (profile.deathParts == null) return;

        for (int i = 0; i < profile.deathParts.Length; i++)
        {
            var prefab = profile.deathParts[i];
            if (prefab == null) continue;

            Quaternion rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
            GameObject part = Instantiate(prefab, transform.position, rotation);

            Rigidbody2D rb = part.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 randomDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(0.5f, 1f)).normalized;
                rb.linearVelocity = randomDirection * profile.spawnForce;
                rb.AddTorque(Random.Range(-profile.torque, profile.torque), ForceMode2D.Impulse);
            }

            Destroy(part, profile.lifeTime);
        }
    }
}
