using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [System.Serializable]
    public class UniverseMusicLayer
    {
        public UniverseId universeId;
        public AudioSource normal; // loop
        public AudioSource battle; // loop (optional)
    }

    [Header("Universe Layers")]
    [SerializeField] private UniverseMusicLayer[] universes;
    [SerializeField] private UniverseId startingUniverse = UniverseId.U1;

    [Header("End Music (optional)")]
    [SerializeField] private AudioSource endMusic;

    [Header("Volumes")]
    [Range(0f, 1f)][SerializeField] private float normalVolume = 1f;
    [Range(0f, 1f)][SerializeField] private float battleVolume = 1f;
    [Range(0f, 1f)][SerializeField] private float endVolume = 1f;

    [Header("Fade Times")]
    [SerializeField] private float universeFadeTime = 0.7f;
    [SerializeField] private float battleFadeTime = 0.5f;
    [SerializeField] private float endFadeTime = 1.0f;

    private readonly Dictionary<UniverseId, UniverseMusicLayer> _layerById = new();
    private readonly List<AudioSource> _allSources = new();

    private UniverseId _currentUniverse;
    private bool _inBattle;
    private bool _gameEnded;

    private Coroutine _stateRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        BuildLookup();
    }

    private void Start()
    {
        // Start everything playing at volume 0 so switching is seamless
        foreach (var layer in universes)
        {
            SafePlayLoop(layer.normal);
            SafePlayLoop(layer.battle);
            SetVol(layer.normal, 0f);
            SetVol(layer.battle, 0f);
        }

        SafePlayLoop(endMusic);
        SetVol(endMusic, 0f);

        _currentUniverse = startingUniverse;

        // Try to sync to UniverseManager if it already exists
        SyncUniverseFromManager();

        // Hook battle tracker
        if (BattleTracker.Instance != null)
            BattleTracker.Instance.OnBattleStateChanged += HandleBattleChanged;

        // Apply initial state immediately
        ApplyStateImmediate(_currentUniverse, inBattle: false);
    }

    private void OnDestroy()
    {
        if (BattleTracker.Instance != null)
            BattleTracker.Instance.OnBattleStateChanged -= HandleBattleChanged;
    }

    private void BuildLookup()
    {
        _layerById.Clear();
        _allSources.Clear();

        foreach (var layer in universes)
        {
            if (layer == null) continue;
            _layerById[layer.universeId] = layer;

            if (layer.normal != null) _allSources.Add(layer.normal);
            if (layer.battle != null) _allSources.Add(layer.battle);
        }

        if (endMusic != null) _allSources.Add(endMusic);
    }

    private void HandleBattleChanged(bool inBattle)
    {
        if (_gameEnded) return;
        _inBattle = inBattle;
        ApplyStateSmooth();
    }

    /// Call this from UniverseManager (or via a bridge script) whenever universe changes.
    public void SetUniverse(UniverseId id)
    {
        if (_gameEnded) return;
        if (_currentUniverse == id) return;

        _currentUniverse = id;

        LogUniverseAudioState(id);

        ApplyStateSmooth();
    }

    /// Useful if scene reloads recreate UniverseManager.
    public void SyncUniverseFromManager()
    {
        if (UniverseManager.Instance != null)
            _currentUniverse = UniverseManager.Instance.CurrentUniverse;
    }

    public void TriggerGameEnd()
    {
        if (_gameEnded) return;
        _gameEnded = true;

        if (_stateRoutine != null) StopCoroutine(_stateRoutine);
        _stateRoutine = StartCoroutine(FadeToEndRoutine());
    }

    private void ApplyStateImmediate(UniverseId universeId, bool inBattle)
    {
        // Everything off
        foreach (var src in _allSources) SetVol(src, 0f);

        // End off during gameplay
        SetVol(endMusic, 0f);

        if (!_layerById.TryGetValue(universeId, out var layer))
        {
            Debug.LogWarning($"[MusicManager] No layer configured for {universeId}");
            return;
        }

        if (!inBattle)
        {
            SetVol(layer.normal, normalVolume);
            SetVol(layer.battle, 0f);
        }
        else
        {
            if (layer.battle != null)
            {
                SetVol(layer.normal, 0f);
                SetVol(layer.battle, battleVolume);
            }
            else
            {
                // If no battle track assigned, keep normal
                SetVol(layer.normal, normalVolume);
            }
        }
    }

    private void ApplyStateSmooth()
    {
        if (_stateRoutine != null) StopCoroutine(_stateRoutine);

        float fadeTime = _inBattle ? battleFadeTime : universeFadeTime;
        _stateRoutine = StartCoroutine(FadeToStateRoutine(_currentUniverse, _inBattle, fadeTime));
    }

    private IEnumerator FadeToStateRoutine(UniverseId universeId, bool inBattle, float fadeTime)
    {
        // Determine targets for each source
        var targets = new Dictionary<AudioSource, float>();

        foreach (var layer in universes)
        {
            if (layer == null) continue;
            if (layer.normal != null) targets[layer.normal] = 0f;
            if (layer.battle != null) targets[layer.battle] = 0f;
        }

        if (endMusic != null) targets[endMusic] = 0f;

        if (_layerById.TryGetValue(universeId, out var active))
        {
            if (!inBattle)
            {
                if (active.normal != null) targets[active.normal] = normalVolume;
                if (active.battle != null) targets[active.battle] = 0f;
            }
            else
            {
                if (active.battle != null)
                {
                    targets[active.normal] = 0f;
                    targets[active.battle] = battleVolume;
                }
                else
                {
                    targets[active.normal] = normalVolume;
                }
            }
        }

        // Snapshot start volumes
        var starts = new Dictionary<AudioSource, float>();
        foreach (var kv in targets)
        {
            if (kv.Key == null) continue;
            starts[kv.Key] = kv.Key.volume;

            // ensure playing & unmuted if target > 0
            if (kv.Value > 0f)
            {
                kv.Key.mute = false;
                if (!kv.Key.isPlaying) kv.Key.Play();
            }
        }

        float t = 0f;
        fadeTime = Mathf.Max(0.01f, fadeTime);

        while (t < fadeTime)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / fadeTime);

            foreach (var kv in targets)
            {
                var src = kv.Key;
                if (src == null) continue;

                float start = starts[src];
                float target = kv.Value;
                src.volume = Mathf.Lerp(start, target, k);
            }

            yield return null;
        }

        // finalize & mute if basically 0
        foreach (var kv in targets)
        {
            var src = kv.Key;
            if (src == null) continue;
            src.volume = kv.Value;
            src.mute = src.volume <= 0.0001f;
        }

        Debug.Log($"Music applied: universe={universeId}, inBattle={inBattle}");
    }

    private IEnumerator FadeToEndRoutine()
    {
        // fade all to 0
        var starts = new Dictionary<AudioSource, float>();
        foreach (var src in _allSources)
        {
            if (src == null) continue;
            starts[src] = src.volume;
        }

        float t = 0f;
        float fadeTime = Mathf.Max(0.01f, endFadeTime);

        // ensure end music playing
        if (endMusic != null)
        {
            endMusic.mute = false;
            if (!endMusic.isPlaying) endMusic.Play();
        }

        while (t < fadeTime)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / fadeTime);

            foreach (var src in _allSources)
            {
                if (src == null) continue;

                float start = starts[src];
                float target = (src == endMusic) ? endVolume : 0f;
                src.volume = Mathf.Lerp(start, target, k);
            }

            yield return null;
        }

        foreach (var src in _allSources)
        {
            if (src == null) continue;
            src.volume = (src == endMusic) ? endVolume : 0f;
            src.mute = src.volume <= 0.0001f;
        }

        
    }

    private static void SafePlayLoop(AudioSource s)
    {
        if (s == null) return;
        s.loop = true;
        if (!s.isPlaying) s.Play();
    }

    private static void SetVol(AudioSource s, float v)
    {
        if (s == null) return;
        s.volume = Mathf.Clamp01(v);
        s.mute = s.volume <= 0.0001f;
    }

    private void LogUniverseAudioState(UniverseId id)
    {
        if (!_layerById.TryGetValue(id, out var layer) || layer == null)
        {
            Debug.LogWarning($"[MusicManager] No layer configured for {id}", this);
            return;
        }

        void LogSource(string label, AudioSource s)
        {
            if (s == null)
            {
                Debug.LogWarning($"[MusicManager] {id} {label}: AudioSource is NULL", this);
                return;
            }

            string clipName = s.clip ? s.clip.name : "NULL_CLIP";
            Debug.Log(
                $"[MusicManager] {id} {label}: " +
                $"clip={clipName}, playing={s.isPlaying}, enabled={s.enabled}, goActive={s.gameObject.activeInHierarchy}, " +
                $"mute={s.mute}, vol={s.volume:0.000}, spatialBlend={s.spatialBlend:0.00}, " +
                $"outputMixer={(s.outputAudioMixerGroup ? s.outputAudioMixerGroup.name : "None")}",
                s
            );
        }

        LogSource("NORMAL", layer.normal);
        LogSource("BATTLE", layer.battle);
    }

}
