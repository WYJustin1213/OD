using System.Collections;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [System.Serializable]
    public class UniverseMusicLayer
    {
        public UniverseId universeId;
        public AudioSource normal;
        public AudioSource battle;
    }

    [Header("Universe Layers")]
    [SerializeField] private UniverseMusicLayer[] universes;
    [SerializeField] private int startingUniverseIndex = 0;

    [Header("End Music")]
    [SerializeField] private AudioSource endMusic; // optional, loop or not

    [Header("Fades")]
    [SerializeField] private float universeFadeTime = 0.6f;
    [SerializeField] private float battleFadeTime = 0.5f;
    [SerializeField] private float endFadeTime = 1.0f;

    [Header("Volumes")]
    [Range(0f, 1f)][SerializeField] private float normalVolume = 1f;
    [Range(0f, 1f)][SerializeField] private float battleVolume = 1f;
    [Range(0f, 1f)][SerializeField] private float endVolume = 1f;

    private int _currentUniverse;
    private bool _inBattle;
    private bool _gameEnded;

    private Coroutine _fadeRoutine;

    private readonly System.Collections.Generic.Dictionary<UniverseId, int> _indexByUniverse
    = new System.Collections.Generic.Dictionary<UniverseId, int>();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _indexByUniverse.Clear();
        for (int i = 0; i < universes.Length; i++)
        {
            _indexByUniverse[universes[i].universeId] = i;
        }

    }

    private void Start()
    {
        // Start everything playing at volume 0 (so switching is seamless).
        for (int i = 0; i < universes.Length; i++)
        {
            SafePlayLoop(universes[i].normal);
            SafePlayLoop(universes[i].battle);
            SetVol(universes[i].normal, 0f);
            SetVol(universes[i].battle, 0f);
        }

        SafePlayLoop(endMusic);
        SetVol(endMusic, 0f);

        _currentUniverse = Mathf.Clamp(startingUniverseIndex, 0, universes.Length - 1);

        // Initial state: normal on, battle off
        ApplyStateImmediate(_currentUniverse, inBattle: false);

        if (UniverseManager.Instance != null)
        {
            // Initialize from current universe
            SetUniverse(UniverseManager.Instance.CurrentUniverse);

            // Listen for changes
            UniverseManager.Instance.UniverseChanged += OnUniverseChanged;
        }

        // Hook battle tracker if present
        if (BattleTracker.Instance != null)
            BattleTracker.Instance.OnBattleStateChanged += HandleBattleChanged;
    }

    private void OnDestroy()
    {
        if (BattleTracker.Instance != null)
            BattleTracker.Instance.OnBattleStateChanged -= HandleBattleChanged;

        if (UniverseManager.Instance != null)
            UniverseManager.Instance.UniverseChanged -= OnUniverseChanged;
    }


    private void OnUniverseChanged(UniverseId oldU, UniverseId newU)
    {
        SetUniverse(newU);
    }


    private void HandleBattleChanged(bool inBattle)
    {
        if (_gameEnded) return;
        _inBattle = inBattle;
        ApplyStateSmooth();
    }

    /// Call this from UniverseManager when universe changes.
    public void SetUniverse(UniverseId id)
    {
        if (_gameEnded) return;

        if (!_indexByUniverse.TryGetValue(id, out int idx))
        {
            Debug.LogWarning($"[MusicManager] No music layer configured for {id}.", this);
            return;
        }

        if (_currentUniverse == idx) return;

        _currentUniverse = idx;
        ApplyStateSmooth();
    }


    /// Call this when game ends (reaching your end object).
    public void TriggerGameEnd()
    {
        if (_gameEnded) return;
        _gameEnded = true;

        if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
        _fadeRoutine = StartCoroutine(FadeToEndRoutine());
    }

    private void ApplyStateSmooth()
    {
        if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
        _fadeRoutine = StartCoroutine(FadeToStateRoutine(_currentUniverse, _inBattle));
    }

    private void ApplyStateImmediate(int universeIndex, bool inBattle)
    {
        // everything off first
        for (int i = 0; i < universes.Length; i++)
        {
            SetVol(universes[i].normal, 0f);
            SetVol(universes[i].battle, 0f);
        }
        SetVol(endMusic, 0f);

        var layer = universes[universeIndex];

        if (!inBattle)
        {
            SetVol(layer.normal, normalVolume);
            SetVol(layer.battle, 0f);
        }
        else
        {
            // If you don't assign a battle track, you can keep normal up.
            if (layer.battle != null)
            {
                SetVol(layer.normal, 0f);
                SetVol(layer.battle, battleVolume);
            }
            else
            {
                SetVol(layer.normal, normalVolume);
            }
        }
    }

    private IEnumerator FadeToStateRoutine(int universeIndex, bool inBattle)
    {
        // Target volumes
        for (int i = 0; i < universes.Length; i++)
        {
            float targetNormal = 0f;
            float targetBattle = 0f;

            if (i == universeIndex)
            {
                if (!inBattle)
                {
                    targetNormal = normalVolume;
                    targetBattle = 0f;
                }
                else
                {
                    // If no battle clip assigned, keep normal up.
                    if (universes[i].battle != null)
                    {
                        targetNormal = 0f;
                        targetBattle = battleVolume;
                    }
                    else
                    {
                        targetNormal = normalVolume;
                        targetBattle = 0f;
                    }
                }
            }

            // Two different fade speeds: universe swap vs battle swap
            float t = (i == universeIndex) ? battleFadeTime : universeFadeTime;
            StartCoroutine(FadeAudio(universes[i].normal, targetNormal, t));
            StartCoroutine(FadeAudio(universes[i].battle, targetBattle, t));
        }

        // Ensure end music off during gameplay
        yield return FadeAudio(endMusic, 0f, 0.2f);
    }

    private IEnumerator FadeToEndRoutine()
    {
        // Fade all universe layers to 0
        for (int i = 0; i < universes.Length; i++)
        {
            StartCoroutine(FadeAudio(universes[i].normal, 0f, endFadeTime));
            StartCoroutine(FadeAudio(universes[i].battle, 0f, endFadeTime));
        }

        // Fade end music up
        yield return FadeAudio(endMusic, endVolume, endFadeTime);
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

    private IEnumerator FadeAudio(AudioSource s, float target, float time)
    {
        if (s == null) yield break;

        target = Mathf.Clamp01(target);

        // unmute before fading in
        if (target > 0f) s.mute = false;

        float start = s.volume;
        float t = 0f;
        time = Mathf.Max(0.01f, time);

        while (t < time)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / time);
            s.volume = Mathf.Lerp(start, target, k);
            yield return null;
        }

        s.volume = target;
        s.mute = s.volume <= 0.0001f;
    }
}
