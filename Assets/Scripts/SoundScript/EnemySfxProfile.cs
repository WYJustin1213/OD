using UnityEngine;

[CreateAssetMenu(menuName = "Audio/Enemy SFX Profile")]
public class EnemySfxProfile : ScriptableObject
{
    [Header("Attack")]
    public SfxClip attack;

    [Header("Damage")]
    public SfxClip damaged;

    [Header("Battle Enter")]
    public SfxClip aggro;

    [Header("Death")]
    public SfxClip death;
}
