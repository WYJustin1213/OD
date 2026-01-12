using UnityEngine;

public class Hurtbox : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private Health health;

    [Header("I-Frames")]
    [SerializeField] private float iFrameTime = 0.25f;
    private float invincibleUntil;

    private void Awake()
    {
        if (player == null) player = GetComponentInParent<Player>();
        if (health == null) health = GetComponentInParent<Health>();
    }

    public void TakeHit(int damage, Transform attacker)
    {
        if (Time.time < invincibleUntil) return;
        invincibleUntil = Time.time + iFrameTime;

        if (health != null)
            health.ChangeHealth(-Mathf.Abs(damage));

        if (player != null)
            player.TakeHitFromEnemy(attacker);

        Debug.Log($"Hurtbox.TakeHit called on {name}, attacker={attacker.name}", this);
    }
}
