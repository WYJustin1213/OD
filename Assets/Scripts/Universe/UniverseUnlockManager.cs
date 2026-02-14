using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class UniverseUnlockManager : MonoBehaviour
{
    public static UniverseUnlockManager Instance { get; private set; }

    [Header("Default state")]
    [Tooltip("If empty, ALL universes are locked by default.")]
    [SerializeField] private UniverseId[] unlockedOnStart;

    private readonly HashSet<UniverseId> _unlocked = new HashSet<UniverseId>();

    public event Action<UniverseId> UniverseUnlocked;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _unlocked.Clear();
        if (unlockedOnStart != null)
        {
            foreach (var u in unlockedOnStart)
                _unlocked.Add(u);
        }
    }

    public bool IsUnlocked(UniverseId u) => _unlocked.Contains(u);

    /// Unlocks a universe. Returns true if it was newly unlocked.
    public bool Unlock(UniverseId u)
    {
        if (_unlocked.Add(u))
        {
            UniverseUnlocked?.Invoke(u);
            return true;
        }
        return false;
    }

    public void LockAll()
    {
        _unlocked.Clear();
    }

    public void Lock(UniverseId u)
    {
        _unlocked.Remove(u);
    }

    // Optional: for debugging/cheats
    public void UnlockAll(params UniverseId[] all)
    {
        foreach (var u in all) Unlock(u);
    }
}
