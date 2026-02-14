using System;
using UnityEngine;

public sealed class UniverseManager : MonoBehaviour
{
    public static UniverseManager Instance { get; private set; }

    [field: SerializeField] public UniverseId CurrentUniverse { get; private set; } = UniverseId.U1;

    public event Action<UniverseId, UniverseId> UniverseWillChange;
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
        DontDestroyOnLoad(gameObject);
    }

    public bool CanSwitchTo(UniverseId target)
    {
        var unlock = UniverseUnlockManager.Instance;

        // If you want switching to require unlock manager:
        // if (unlock == null) return false;

        if (unlock == null) return true;
        return unlock.IsUnlocked(target);
    }

    public bool TrySetUniverse(UniverseId newUniverse)
    {
        if (_isSwitching) return false;
        if (newUniverse == CurrentUniverse) return false;

        // ✅ enforce lock here (prevents bypass)
        if (!CanSwitchTo(newUniverse)) return false;

        _isSwitching = true;

        UniverseId old = CurrentUniverse;

        try
        {
            UniverseWillChange?.Invoke(old, newUniverse);

            CurrentUniverse = newUniverse;

            // ✅ Keep this if you want guaranteed sync
            MusicManager.Instance?.SetUniverse(CurrentUniverse);

            if (logChanges)
                Debug.Log($"Universe changed: {old} -> {newUniverse}", this);

            UniverseChanged?.Invoke(old, newUniverse);
        }
        finally
        {
            _isSwitching = false;
        }

        return true;
    }
}
