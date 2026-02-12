using UnityEngine;

[CreateAssetMenu(menuName = "Audio/SFX Clip")]
public class SfxClip : ScriptableObject
{
    [Header("Clips (random pick)")]
    public AudioClip[] clips;

    [Header("Volume")]
    [Range(0f, 1f)] public float volume = 1f;

    [Header("Pitch Randomization")]
    public bool randomPitch = true;
    [Range(0.5f, 2f)] public float pitchMin = 0.95f;
    [Range(0.5f, 2f)] public float pitchMax = 1.05f;

    [Header("Delay (optional default)")]
    [Min(0f)] public float defaultDelay = 0f;

    public AudioClip Pick()
    {
        if (clips == null || clips.Length == 0) return null;
        int i = Random.Range(0, clips.Length);
        return clips[i];
    }

    public float PickPitch()
    {
        if (!randomPitch) return 1f;
        return Random.Range(pitchMin, pitchMax);
    }
}
