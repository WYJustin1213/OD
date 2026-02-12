using UnityEngine;

[CreateAssetMenu(menuName = "Audio/Player SFX Profile")]
public class PlayerSfxProfile : ScriptableObject
{
    [Header("Attack 1")]
    public SfxClip attack1Start;
    public SfxClip attack1Hit;

    [Header("Attack 2")]
    public SfxClip attack2Start;
    public SfxClip attack2Hit;

    [Header("Attack 3")]
    public SfxClip attack3Start;
    public SfxClip attack3Hit;

    [Header("Landing")]
    public SfxClip land;

    [Header("Movement")]
    public SfxClip walk;
    public SfxClip run;

    [Header("Hit")]
    public SfxClip hit;
    public SfxClip death;
}
