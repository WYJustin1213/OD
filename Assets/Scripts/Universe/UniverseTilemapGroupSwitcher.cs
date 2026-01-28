using UnityEngine;

public class UniverseTilemapGroupSwitcher : MonoBehaviour
{
    [Header("Parents that contain tilemap layers for each universe")]
    [SerializeField] private GameObject universe1Root;
    [SerializeField] private GameObject universe2Root;
    [SerializeField] private GameObject universe3Root;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    private UniverseManager _um;

    private void Awake()
    {
        // Grab in Awake; UniverseManager might not exist yet, so we also re-check in Start.
        _um = UniverseManager.Instance;
    }

    private void Start()
    {
        // Ensure we have UniverseManager by Start
        if (_um == null) _um = UniverseManager.Instance;

        if (_um == null)
        {
            Debug.LogError("UniverseTilemapGroupSwitcher: UniverseManager.Instance is null. Is UniverseManager in the scene and enabled?", this);
            return;
        }

        _um.UniverseChanged += OnUniverseChanged;

        // Apply initial state
        Apply(_um.CurrentUniverse);
    }

    private void OnDestroy()
    {
        if (_um != null)
            _um.UniverseChanged -= OnUniverseChanged;
    }

    private void OnUniverseChanged(UniverseId oldU, UniverseId newU)
    {
        Apply(newU);
    }

    private void Apply(UniverseId u)
    {
        if (universe1Root == null || universe2Root == null)
        {
            Debug.LogError("UniverseTilemapGroupSwitcher: Assign universe1Root and universe2Root in the Inspector.", this);
            return;
        }

        bool u1Active = (u == UniverseId.U1);
        bool u2Active = (u == UniverseId.U2);
        bool u3Active = (u == UniverseId.U3);

        if (debugLogs)
        {
            Debug.Log($"[TilemapSwitcher] Applying {u}: U1Root -> {u1Active}, U2Root -> {u2Active}, U3Root -> {u3Active}", this);
            Debug.Log($"U1Root name={universe1Root.name} activeBefore={universe1Root.activeSelf}", this);
            Debug.Log($"U2Root name={universe2Root.name} activeBefore={universe2Root.activeSelf}", this);
            Debug.Log($"U3Root name={universe3Root.name} activeBefore={universe3Root.activeSelf}", this);
        }

        universe1Root.SetActive(u1Active);
        universe2Root.SetActive(u2Active);
        universe3Root.SetActive(u3Active);

        if (debugLogs)
        {
            Debug.Log($"U1Root activeAfter={universe1Root.activeSelf}", this);
            Debug.Log($"U2Root activeAfter={universe2Root.activeSelf}", this);
            Debug.Log($"U3Root activeAfter={universe3Root.activeSelf}", this);
        }
    }
}
