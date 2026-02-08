using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class GameEndTrigger : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";

    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        MusicManager.Instance?.TriggerGameEnd();
    }
}
