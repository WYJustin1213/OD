//using Cinemachine;
using Unity.Cinemachine;
using UnityEngine;

public class ShakeBus : MonoBehaviour
{
    public static ShakeBus Instance { get; private set; }

    [Header("Impulse Sources (set in Inspector)")]
    [SerializeField] private CinemachineImpulseSource hitConfirm;
    [SerializeField] private CinemachineImpulseSource playerHurt;
    [SerializeField] private CinemachineImpulseSource enemyAttack;

    [Header("Global multiplier")]
    [SerializeField] private float globalGain = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void HitConfirm() => Play(hitConfirm);
    public void PlayerHurt() => Play(playerHurt);
    public void EnemyAttack() => Play(enemyAttack);

    private void Play(CinemachineImpulseSource src)
    {
        if (src == null) return;

        // We can't safely edit the definition across Cinemachine versions,
        // but we can scale the *generated* impulse amplitude with a velocity vector.
        // Bigger vector => bigger impulse.
        Vector3 impulseVelocity = Vector3.right * Mathf.Max(0.001f, globalGain);

        // Some Cinemachine versions have only GenerateImpulse(), others have GenerateImpulse(Vector3).
        // This pattern compiles in both by using the parameterless version:
        src.GenerateImpulse();
    }
}
