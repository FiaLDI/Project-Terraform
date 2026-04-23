using UnityEngine;
using FishNet;
using FishNet.Object;
using Features.Effects.Application;
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
    private NetworkObject _networkObject;
    private static readonly int SpawnPositionID = Shader.PropertyToID("SpawnPosition");

    private void Awake()
    {
        _networkObject = GetComponent<NetworkObject>();
        _vfx = GetComponentInChildren<VisualEffect>();
        if (_vfx == null)
            Debug.LogError("[PulseFX] No VisualEffect component found in children!", this);

        Debug.Log("[PulseFX] Awake on " + name, this);
    }

    private void Start()
    {
        var owner = default(Transform);
        float fxDuration = duration;

        var ctx = GetComponent<SpawnedObjectContext>();
        if (ctx != null)
        {
            if (ctx.Source != null)
                owner = ctx.Source.transform;

            if (ctx.Lifetime > 0f)
                fxDuration = ctx.Lifetime;
        }

        Init(owner, 0f, fxDuration);
    }

    public void Init(Transform owner, float radius, float fxDuration)
    {
        _initialized = true;
        _owner = owner;

        duration = fxDuration > 0f ? fxDuration : duration;
        _deathTime = Time.time + duration;

        Debug.Log($"[PulseFX] Init. duration={duration}", this);

        ApplyVfxPosition();
    }

    private void Update()
    {
        if (!_initialized)
            return;

        if (followOwner && _owner != null && ShouldDriveFollow())
        {
            Vector3 pos = _owner.position;
            pos.y = transform.position.y;
            transform.position = pos;
        }

        ApplyVfxPosition();

        if (ShouldSelfDestroy() && Time.time >= _deathTime)
        {
            Destroy(gameObject);
        }
    }

    private bool ShouldDriveFollow()
    {
        return _networkObject == null || InstanceFinder.IsServerStarted;
    }

    private bool ShouldSelfDestroy()
    {
        return _networkObject == null;
    }

    private void ApplyVfxPosition()
    {
        if (_vfx != null)
            _vfx.SetVector3(SpawnPositionID, transform.position);
    }
}
