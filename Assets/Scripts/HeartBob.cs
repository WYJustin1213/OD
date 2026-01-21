using UnityEngine;

public class HeartBob : MonoBehaviour
{
    [SerializeField] private float amplitude = 0.02f;
    [SerializeField] private float speed = 2.0f;

    private Vector3 _startLocalPos;
    private float _phase;

    public void Configure(float amp, float spd)
    {
        amplitude = amp;
        speed = spd;
    }

    private void OnEnable()
    {
        // OnEnable is better than Awake here because we may be instantiated then laid out.
        _startLocalPos = transform.localPosition;
        _phase = Random.value * 100f;
    }

    private void LateUpdate()
    {
        float y = Mathf.Sin(Time.time * speed + _phase) * amplitude;
        transform.localPosition = _startLocalPos + new Vector3(0f, y, 0f);
    }

    private void Update()
    {
        //Debug.Log(_startLocalPos);
    }
}
