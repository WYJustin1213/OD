using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private Animator animator;
    public Health health;

    private void OnEnable()
    {
        //health.OnDamaged += PlayHit;
    }

    private void OnDisable()
    {
        //health.OnDamaged -= PlayHit;
    }

    private void Update()
    {
        
    }

    public void SetAnimator(Animator newAnimator) => animator = newAnimator;

    public void PlayHit()
    {
        if (animator != null)
            animator.SetTrigger("isHit");
    }

}
