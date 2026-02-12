using UnityEngine;

public class EnemySfxController : MonoBehaviour
{
    [SerializeField] private EnemyCombatController combat;
    [SerializeField] private Health health;

    [Header("Variant profile (set by your variant switcher)")]
    [SerializeField] private EnemySfxProfile profile;

    private bool _wasTargeting;

    private void Awake()
    {
        if (combat == null) combat = GetComponent<EnemyCombatController>();
        if (health == null) health = GetComponent<Health>();
    }

    private void OnEnable()
    {
        if (health != null)
        {
            health.OnDamaged += OnDamaged;
            health.OnDeath += OnDeath;
        }
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.OnDamaged -= OnDamaged;
            health.OnDeath -= OnDeath;
        }
    }

    private void Update()
    {
        if (combat == null) return;

        // We use the same targeting flag you already maintain:
        bool targeting = GetTargetingSafe();

        // Trigger "enter battle" once when targeting begins
        if (!_wasTargeting && targeting)
        {
            // Enemies only: aggro sound
            SfxManager.Instance?.PlayAt(profile != null ? profile.aggro : null, transform.position);
        }

        _wasTargeting = targeting;
    }

    private bool GetTargetingSafe()
    {
        // If you have _isTargetingPlayer private, expose it:
        // public bool IsTargetingPlayer => _isTargetingPlayer;
        // For now, we’ll assume you add that property.
        return combat.IsTargetingPlayer;
    }

    private void OnDamaged()
    {
        // Different per variant
        SfxManager.Instance?.PlayAt(profile != null ? profile.damaged : null, transform.position);
    }

    private void OnDeath()
    {
        SfxManager.Instance?.PlayAt(profile != null ? profile.death : null, transform.position);
    }

    // Call from enemy attack animation event:
    public void Sfx_Attack()
    {
        SfxManager.Instance?.PlayAt(profile != null ? profile.attack : null, transform.position);
    }

    // Variant switcher can call this when universe changes:
    public void SetProfile(EnemySfxProfile p) => profile = p;
}
