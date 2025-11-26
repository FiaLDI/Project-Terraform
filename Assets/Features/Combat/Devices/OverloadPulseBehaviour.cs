using UnityEngine;

public class OverloadPulseBehaviour : MonoBehaviour
{
    [Header("Settings")]
    public float duration = 0.35f;

    [Header("Follow Owner")]
    public bool followOwner = true;

    private float _deathTime;
    private bool _initialized;
    private Transform _owner;

    private void Awake()
    {
        Debug.Log("[PulseFX] Awake on " + name, this);
    }

    public void Init(Transform owner, float radius, float fxDuration)
    {
        _initialized = true;

        _owner = owner;
        duration = fxDuration;

        _deathTime = Time.time + duration;

        Debug.Log($"[PulseFX] Init. duration={duration}, deathTime={_deathTime}", this);
    }

    private void Update()
    {
        if (!_initialized)
            return;

        if (followOwner && _owner)
        {
            Vector3 pos = _owner.position;
            pos.y = transform.position.y;
            transform.position = pos;
        }

        if (Time.time >= _deathTime)
        {
            Destroy(gameObject);
        }
    }
}
