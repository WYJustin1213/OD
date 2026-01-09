using UnityEngine;

public class EnemyDeathFxProfile : MonoBehaviour
{
    [Header("Death FX (sprite-specific)")]
    public GameObject[] deathParts;
    public float spawnForce = 4f;
    public float torque = 5f;
    public float lifeTime = 1.5f;
}
