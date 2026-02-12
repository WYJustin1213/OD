using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SfxManager : MonoBehaviour
{
    public static SfxManager Instance { get; private set; }

    [Header("Pool")]
    [SerializeField] private int poolSize = 16;
    [SerializeField] private AudioSource sourcePrefab;

    [Header("Output")]
    [SerializeField] private AudioMixerGroupRef output; // optional helper below; safe if null

    private readonly Queue<AudioSource> _pool = new();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        WarmPool();
    }

    private void WarmPool()
    {
        if (sourcePrefab == null)
        {
            // Create a default prefab-like source if none assigned
            GameObject go = new GameObject("SfxSourcePrefab");
            go.transform.SetParent(transform);
            var a = go.AddComponent<AudioSource>();
            a.playOnAwake = false;
            a.loop = false;
            a.spatialBlend = 0f; // 2D by default
            sourcePrefab = a;
        }

        for (int i = 0; i < poolSize; i++)
        {
            var s = Instantiate(sourcePrefab, transform);
            s.name = $"SfxSource_{i}";
            s.playOnAwake = false;
            s.loop = false;
            s.clip = null;
            if (output != null && output.group != null) s.outputAudioMixerGroup = output.group;
            _pool.Enqueue(s);
        }
    }

    private AudioSource GetSource()
    {
        if (_pool.Count == 0)
        {
            var s = Instantiate(sourcePrefab, transform);
            s.playOnAwake = false;
            s.loop = false;
            if (output != null && output.group != null) s.outputAudioMixerGroup = output.group;
            return s;
        }
        return _pool.Dequeue();
    }

    private void ReturnSource(AudioSource s)
    {
        if (s == null) return;
        s.Stop();
        s.clip = null;
        _pool.Enqueue(s);
    }

    /// 2D play (ignores position)
    public void Play2D(SfxClip sfx, float delayOverride = -1f)
    {
        PlayAt(sfx, Vector3.zero, usePosition: false, delayOverride);
    }

    /// Play at a world position (3D if your prefab uses spatialBlend > 0)
    public void PlayAt(SfxClip sfx, Vector3 pos, float delayOverride = -1f)
    {
        PlayAt(sfx, pos, usePosition: true, delayOverride);
    }

    private void PlayAt(SfxClip sfx, Vector3 pos, bool usePosition, float delayOverride)
    {
        if (sfx == null) return;

        AudioClip clip = sfx.Pick();
        if (clip == null) return;

        float delay = (delayOverride >= 0f) ? delayOverride : sfx.defaultDelay;

        StartCoroutine(PlayRoutine(clip, sfx.volume, sfx.PickPitch(), pos, usePosition, delay));
    }

    private IEnumerator PlayRoutine(AudioClip clip, float volume, float pitch, Vector3 pos, bool usePosition, float delay)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);

        AudioSource src = GetSource();
        if (usePosition) src.transform.position = pos;

        src.pitch = pitch;
        src.volume = volume;

        // Use PlayOneShot so we don't have to manage clip assignment lifetime
        src.PlayOneShot(clip);

        // Wait for clip length adjusted by pitch
        float t = clip.length / Mathf.Max(0.01f, Mathf.Abs(pitch));
        yield return new WaitForSeconds(t);

        ReturnSource(src);
    }
}
