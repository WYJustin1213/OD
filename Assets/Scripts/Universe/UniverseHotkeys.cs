using UnityEngine;

public class UniverseHotkeys : MonoBehaviour
{
    private void Update()
    {
        if (UniverseManager.Instance == null) return;

        if (!Input.GetKey(KeyCode.U)) return; // U must be held

        if (Input.GetKeyDown(KeyCode.Alpha1))
            UniverseManager.Instance.TrySetUniverse(UniverseId.U1);

        if (Input.GetKeyDown(KeyCode.Alpha2))
            UniverseManager.Instance.TrySetUniverse(UniverseId.U2);
    }
}
