using UnityEngine;

public class EnemyCombatEventProxy : MonoBehaviour
{
    [SerializeField] private EnemyCombatController controller;

    private void Awake()
    {
        if (controller == null)
            controller = GetComponentInParent<EnemyCombatController>();
    }

    public void DealDamage() => controller.DealDamage();
    public void EndAttack() => controller.EndAttack();
}
