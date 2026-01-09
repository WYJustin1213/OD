using UnityEngine;

public class EnemyVariantCombatRefs : MonoBehaviour
{
    [Header("Variant-specific references")]
    public Animator animator;
    public Transform attackPoint;

    [Header("Optional overrides (leave <= 0 to ignore)")]
    public float attackRangeOverride = -1f;
    public float attackRadiusOverride = -1f;
}
