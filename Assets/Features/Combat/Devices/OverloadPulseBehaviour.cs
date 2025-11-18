using UnityEngine;

public class OverloadPulseBehaviour : MonoBehaviour
{
    private float _radius;
    private float _duration;
    private float _deathTime;
    private Transform _owner;
    private EnergyConsumer energyConsumer;

    private float _startScale = 0.2f;

    private void Awake()
    {
        energyConsumer = GetComponent<EnergyConsumer>();
    }

    public void Init(Transform owner, float radius, float duration)
    {
        _owner = owner;
        _radius = radius;
        _duration = duration;

        _deathTime = Time.time + 1f; // FX живёт мало
        transform.localScale = Vector3.one * _startScale;
    }

    private void Update()
    {
        if (_owner != null)
        {
            transform.position = _owner.position;
        }

        // expand pulse
        float t = Mathf.Clamp01(1f - ((_deathTime - Time.time) / 1f));
        float scale = Mathf.Lerp(_startScale, _radius * 2f, t);
        transform.localScale = Vector3.one * scale;

        if (Time.time >= _deathTime)
            Destroy(gameObject);
    }
}
