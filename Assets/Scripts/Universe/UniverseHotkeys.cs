using UnityEngine;

public class UniverseHotkeys : MonoBehaviour
{
    private void Update()
    {
        if (UniverseManager.Instance == null) return;

        bool uHeld = Input.GetKey(KeyCode.U);

        if (uHeld && Input.GetKeyDown(KeyCode.Alpha1))
            UniverseManager.Instance.TrySetUniverse(UniverseId.U1);

        if (uHeld && Input.GetKeyDown(KeyCode.Alpha2))
            UniverseManager.Instance.TrySetUniverse(UniverseId.U2);
    }
}
