using UnityEngine;

public class ChargeDeviceBehaviour : MonoBehaviour
{
    private float _duration;
    private Transform _owner;
    private float _deathTime;
    private EnergyConsumer energyConsumer;

    private void Awake()
    {
        energyConsumer = GetComponent<EnergyConsumer>();
    }

    public void Init(Transform owner, float duration)
    {
        _owner = owner;
        _duration = duration;
        _deathTime = Time.time + duration;
    }

    private void Update()
    {
        if (_owner != null)
        {
            // FX следует за игроком
            transform.position = _owner.position;
        }

        if (Time.time >= _deathTime)
        {
            Destroy(gameObject);
        }
    }
}
