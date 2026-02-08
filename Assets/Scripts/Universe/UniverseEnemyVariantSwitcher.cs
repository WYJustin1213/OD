using UnityEngine;

public class UniverseEnemyVariantSwitcher : MonoBehaviour
{
    [System.Serializable]
    public class Variant
    {
        public UniverseId universe;
        public GameObject root;          // Variant_U1 / Variant_U2 child object
        public Animator animator;        // Animator inside that variant
        public Transform attackPoint;    // Attack point inside that variant (optional)

        // Optional: per-variant motor check origins (useful if sprite size changes)
        public Transform groundCheckOrigin;
        public Transform wallCheckOrigin;
    }

    [Header("Variants")]
    [SerializeField] private Variant[] variants;

    [Header("Scripts to update")]
    [SerializeField] private Enemy enemyFx;                       // your Enemy script (uses animator for isHit)
    [SerializeField] private EnemyCombatController combat;        // uses animator + attackPoint
    [SerializeField] private EnemyMotor2D motor;                  // may use ground/wall origins
    [SerializeField] private EnemyIdleController idle;

    private UniverseManager _um;

    private Variant _activeVariant;


    private void Awake()
    {
        if (enemyFx == null) enemyFx = GetComponent<Enemy>();
        if (combat == null) combat = GetComponent<EnemyCombatController>();
        if (motor == null) motor = GetComponent<EnemyMotor2D>();
        if (idle == null) idle = GetComponent<EnemyIdleController>();
    }

    private void Start()
    {
        _um = UniverseManager.Instance;

        if (_um == null)
        {
            Debug.LogError("UniverseEnemyVariantSwitcher: UniverseManager.Instance is null.", this);
            return;
        }

        _um.UniverseChanged += OnUniverseChanged;

        // Apply initial universe
        Apply(_um.CurrentUniverse);
    }

    private void OnDestroy()
    {
        if (_um != null) _um.UniverseChanged -= OnUniverseChanged;
    }

    private void OnUniverseChanged(UniverseId oldU, UniverseId newU)
    {
        Apply(newU);
    }

    private void Apply(UniverseId u)
    {
        Variant active = null;

        // Enable only the active variant
        for (int i = 0; i < variants.Length; i++)
        {
            bool isActive = variants[i] != null && variants[i].universe == u;
            if (variants[i]?.root != null)
                variants[i].root.SetActive(isActive);

            if (isActive) active = variants[i];

            _activeVariant = active;
        }

        if (active == null)
        {
            //Debug.LogWarning($"UniverseEnemyVariantSwitcher: No variant configured for {u} on {name}", this);
            return;
        }

        // Update animator references so hit/attack works in the current universe
        if (enemyFx != null && active.animator != null)
            enemyFx.SetAnimator(active.animator);

        _activeVariant = active;

        var refs = active.root.GetComponentInChildren<EnemyVariantCombatRefs>(includeInactive: false);
        if (refs == null)
        {
           // Debug.LogWarning($"UniverseEnemyVariantSwitcher: No EnemyVariantCombatRefs found under {active.root.name}", this);
            return;
        }

        combat.ApplyVariantAnimatorAndAttackPoint(refs.animator, refs.attackPoint);
        idle.SetAnimator(refs.animator);
        enemyFx.SetAnimator(refs.animator);

        //Debug.Log($"[VariantSwitcher] {name} applying {u}. refs={(refs != null)} animator={(refs != null ? refs.animator : null)} attackPoint={(refs != null ? refs.attackPoint : null)}", this);

        if (combat != null)
            combat.ApplyVariantAnimatorAndAttackPoint(refs.animator, refs.attackPoint);

        // Helps physics update when colliders swap
        Physics2D.SyncTransforms();


        // Update motor check origins if you use those (optional)
        if (motor != null)
        {
            if (active.groundCheckOrigin != null) motor.SetGroundCheckOrigin(active.groundCheckOrigin);
            if (active.wallCheckOrigin != null) motor.SetWallCheckOrigin(active.wallCheckOrigin);
        }

        // Helps Physics2D register collider enable/disable immediately
        Physics2D.SyncTransforms();
    }

    public EnemyDeathFxProfile GetActiveDeathFxProfile()
    {
        if (_activeVariant == null || _activeVariant.root == null) return null;
        return _activeVariant.root.GetComponentInChildren<EnemyDeathFxProfile>(includeInactive: false);
    }

}
