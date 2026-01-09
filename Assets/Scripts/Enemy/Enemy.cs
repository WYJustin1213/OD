using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private Animator animator;
    public Health health;

    private void OnEnable()
    {
        health.OnDamaged += HandleDamage;
    }

    private void OnDisable()
    {
        health.OnDamaged -= HandleDamage;
    }

    public void SetAnimator(Animator newAnimator) => animator = newAnimator;

    void HandleDamage()
    {
        if (animator != null)
            animator.SetTrigger("isHit");
    }
}
