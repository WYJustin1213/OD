using System;
using UnityEngine;

public sealed class UniverseManager : MonoBehaviour
{
    public static UniverseManager Instance { get; private set; }

    [field: SerializeField] public UniverseId CurrentUniverse { get; private set; } = UniverseId.U1;

    // Universe Will Change: Fired before systems swap (for cleanup / caching).
    public event Action<UniverseId, UniverseId> UniverseWillChange;

    // Universe Changed: Fired after systems swap (applying visuals / ai / forms).
    public event Action<UniverseId, UniverseId> UniverseChanged;

    [SerializeField] private bool logChanges = false;
    private bool _isSwitching;



    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        // DontDestroyOnLoad(gameObject);
    }



    public bool TrySetUniverse(UniverseId newUniverse)
    {
        if (_isSwitching)
        { return false; }

        if (newUniverse == CurrentUniverse)
        { return false; }

        _isSwitching = true;

        UniverseId old = CurrentUniverse;

        try
        {
            UniverseWillChange?.Invoke(old, newUniverse);

            CurrentUniverse = newUniverse;

            if (logChanges)
            { Debug.Log($"Universe changed: {old} -> {newUniverse}", this); }

            UniverseChanged?.Invoke(old, newUniverse);
        }
        finally
        {
            _isSwitching = false;
        }

        return true;
    }

    public void CycleUniverse()
    {
        // Example cycling logic; customize as needed.
        UniverseId next = CurrentUniverse == UniverseId.U1 ? UniverseId.U2 : UniverseId.U1;
        TrySetUniverse(next);
    }
}
