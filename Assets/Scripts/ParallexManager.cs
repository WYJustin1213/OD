using UnityEngine;
using UnityEngine.Rendering;

public class ParallexManager : MonoBehaviour
{
    [System.Serializable]
    public class ParallaxLayer
    {
        public Transform layer;
        [Range(0, 1)] public float parallaxFactor;
    }

    public ParallaxLayer[] layers;

    public Transform camTrans;
    private Vector3 lastCamPos;

    private void Start()
    {
        lastCamPos = camTrans.position;
    }

    private void LateUpdate()
    {
        Vector3 cameraDelta = camTrans.position - lastCamPos;

        foreach (ParallaxLayer layer in layers)
        {
            float moveX = cameraDelta.x * layer.parallaxFactor;
            //float moveY = cameraDelta.y * layer.parallaxFactor;

            layer.layer.position += new Vector3(moveX, 0, 0);
        }

        lastCamPos = camTrans.position;
    }
}
