using UnityEngine;
using UnityEngine.VFX;

public class OverloadPulseBehaviour : MonoBehaviour
{
    [Header("Settings")]
    public float duration = 0.35f;

    [Header("Follow Owner")]
    public bool followOwner = true;

    private Transform _owner;
    private float _deathTime;
    private bool _initialized;

    private VisualEffect _vfx;
    private static readonly int SpawnPositionID = Shader.PropertyToID("SpawnPosition");

    private void Awake()
    {
        _vfx = GetComponentInChildren<VisualEffect>();
        if (_vfx == null)
            Debug.LogError("[PulseFX] No VisualEffect component found in children!", this);

        Debug.Log("[PulseFX] Awake on " + name, this);
    }

    public void Init(Transform owner, float radius, float fxDuration)
    {
        _initialized = true;
        _owner = owner;

        duration = fxDuration;
        _deathTime = Time.time + duration;

        Debug.Log($"[PulseFX] Init. duration={duration}", this);

        if (_vfx != null)
            _vfx.SetVector3(SpawnPositionID, transform.position);
    }

    private void Update()
    {
        if (!_initialized)
            return;

        if (followOwner && _owner != null)
        {
            Vector3 pos = _owner.position;
            pos.y = transform.position.y;
            transform.position = pos;

            if (_vfx != null)
                _vfx.SetVector3(SpawnPositionID, transform.position);
        }

        if (Time.time >= _deathTime)
        {
            Destroy(gameObject);
        }
    }
}
