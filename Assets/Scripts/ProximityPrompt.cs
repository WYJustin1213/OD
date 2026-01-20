using UnityEngine;
using TMPro;

//[RequireComponent(typeof(Collider2D))]
public class ProximityPrompt : MonoBehaviour
{
    [Header("Prompt")]
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private string message = "Press E";
    [SerializeField] private float showDistance = 1.5f;

    [Header("Target")]
    [SerializeField] private Transform player;

    private void Awake()
    {
        if (promptText != null)
            promptText.gameObject.SetActive(false);

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    private void Update()
    {
        if (player == null || promptText == null) return;

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist <= showDistance)
        {
            if (!promptText.gameObject.activeSelf)
            {
                promptText.text = message;
                promptText.gameObject.SetActive(true);
            }
        }
        else
        {
            if (promptText.gameObject.activeSelf)
                promptText.gameObject.SetActive(false);
        }
    }
}
