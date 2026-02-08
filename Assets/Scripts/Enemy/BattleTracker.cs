using System;
using UnityEngine;

public class BattleTracker : MonoBehaviour
{
    public static BattleTracker Instance { get; private set; }

    public event Action<bool> OnBattleStateChanged; // true=in battle

    [SerializeField] private float battleEndGraceSeconds = 3f;

    private int _targetingCount;
    private float _battleOffAtTime;
    private bool _inBattle;

    public bool InBattle => _inBattle;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (_inBattle && _targetingCount <= 0 && Time.time >= _battleOffAtTime)
            SetBattle(false);
    }

    /// Call when an enemy starts targeting the player.
    public void NotifyTargetingStarted()
    {
        _targetingCount++;
        if (_targetingCount == 1)
            SetBattle(true);
    }

    /// Call when an enemy stops targeting (lost aggro / disengage / died).
    public void NotifyTargetingStopped()
    {
        _targetingCount = Mathf.Max(0, _targetingCount - 1);

        if (_targetingCount == 0)
            _battleOffAtTime = Time.time + battleEndGraceSeconds;
    }

    private void SetBattle(bool battle)
    {
        if (_inBattle == battle) return;
        _inBattle = battle;
        OnBattleStateChanged?.Invoke(_inBattle);
    }
}
