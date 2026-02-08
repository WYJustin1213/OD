using UnityEngine;

public class UniverseMusicBridge : MonoBehaviour
{
    private void OnEnable()
    {
        if (UniverseManager.Instance != null)
            UniverseManager.Instance.UniverseChanged += OnUniverseChanged;

        // initial sync
        if (MusicManager.Instance != null && UniverseManager.Instance != null)
            MusicManager.Instance.SetUniverse(UniverseManager.Instance.CurrentUniverse);
    }

    private void OnDisable()
    {
        if (UniverseManager.Instance != null)
            UniverseManager.Instance.UniverseChanged -= OnUniverseChanged;
    }

    private void OnUniverseChanged(UniverseId oldU, UniverseId newU)
    {
        MusicManager.Instance?.SetUniverse(newU);
    }
}
