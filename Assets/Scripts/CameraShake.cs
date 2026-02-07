using UnityEngine;

/// <summary>
/// Lightweight screen shake (no Cinemachine).
/// Put on the camera you want to shake.
/// Call CameraShake.Instance.Shake(...)
/// </summary>
public class CameraShake : MonoBehaviour
{
    /*
    public static CameraShake Instance { get; private set; }

    [Header("Global multipliers")]
    [SerializeField] private float strengthMultiplier = 1f;
    [SerializeField] private float timeMultiplier = 1f;

    [Header("Default settings")]
    [SerializeField] private float defaultFrequency = 25f;
    [SerializeField] private float maxOffset = 0.6f; // safety clamp

    private Vector3 _baseLocalPos;

    // Current shake state (we blend multiple requests into these)
    private float _timeLeft;
    private float _duration;
    private float _magnitude;
    private float _frequency;
    private float _seed;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;

        _baseLocalPos = transform.localPosition;
        _seed = Random.value * 1000f;
    }

    private void OnEnable()
    {
        _baseLocalPos = transform.localPosition;
    }

    private void LateUpdate()
    {
        if (_timeLeft <= 0f)
        {
            transform.localPosition = _baseLocalPos;
            return;
        }

        _timeLeft -= Time.deltaTime;
        float t01 = (_duration <= 0f) ? 0f : Mathf.Clamp01(_timeLeft / _duration);

        // Smooth falloff (feels better than linear)
        float falloff = t01 * t01;

        float time = Time.time * _frequency;
        float x = (Mathf.PerlinNoise(_seed, time) - 0.5f) * 2f;
        float y = (Mathf.PerlinNoise(_seed + 10.73f, time) - 0.5f) * 2f;

        Vector3 offset = new Vector3(x, y, 0f) * (_magnitude * falloff);

        // Clamp so it never goes insane
        offset.x = Mathf.Clamp(offset.x, -maxOffset, maxOffset);
        offset.y = Mathf.Clamp(offset.y, -maxOffset, maxOffset);

        transform.localPosition = _baseLocalPos + offset;
    }

    /// <summary>
    /// Request a shake. Multiple calls blend by taking the stronger magnitude and longer remaining time.
    /// </summary>
    public void Shake(float duration, float magnitude, float frequency = -1f)
    {
        duration *= timeMultiplier;
        magnitude *= strengthMultiplier;
        if (frequency <= 0f) frequency = defaultFrequency;

        // If new shake is stronger, replace magnitude.
        // If new shake lasts longer, extend time.
        _magnitude = Mathf.Max(_magnitude, magnitude);
        _frequency = Mathf.Max(_frequency, frequency);

        // Extend time/duration if needed
        float newTimeLeft = Mathf.Max(_timeLeft, duration);
        if (newTimeLeft > _timeLeft)
        {
            _timeLeft = newTimeLeft;
            _duration = Mathf.Max(_duration, duration);
        }
        else
        {
            // still update duration to keep falloff reasonable
            _duration = Mathf.Max(_duration, duration);
        }
    }

    /// <summary>Optional: call this if the camera moves and you want shake relative to its new position.</summary>
    public void Rebase()
    {
        _baseLocalPos = transform.localPosition;
    }
    */
}
