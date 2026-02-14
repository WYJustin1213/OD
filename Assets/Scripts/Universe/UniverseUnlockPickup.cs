using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class UniverseUnlockPickup : MonoBehaviour
{
    [Header("Unlock")]
    [SerializeField] private UniverseId unlockUniverse = UniverseId.U2;

    [Header("Who can unlock")]
    [SerializeField] private string playerTag = "Player";

    [Header("Disable on pickup")]
    [SerializeField] private bool disableObject = true;

    private void Reset()
    {
        var c = GetComponent<Collider2D>();
        c.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        var unlock = UniverseUnlockManager.Instance;
        if (unlock == null)
        {
            Debug.LogWarning("No UniverseUnlockManager in scene. Add one to enable unlocking.");
            return;
        }

        bool newlyUnlocked = unlock.Unlock(unlockUniverse);

        if (newlyUnlocked)
            Debug.Log($"Unlocked universe: {unlockUniverse}");

        if (disableObject)
            gameObject.SetActive(false);
    }
}
