using UnityEngine;

public static class ShakePresets
{
    // Player hits enemy
    public static readonly (float dur, float mag, float freq) HitConfirm = (0.08f, 0.12f, 30f);

    // Player gets hit
    public static readonly (float dur, float mag, float freq) PlayerHurt = (0.12f, 0.22f, 28f);

    // Enemy attack (windup / slam)
    public static readonly (float dur, float mag, float freq) EnemyAttack = (0.10f, 0.16f, 26f);
}
