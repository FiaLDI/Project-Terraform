using UnityEngine;
using System.Collections;
using FishNet.Object;
using Features.Stats.Domain;
using Features.Effects.Domain;
using Features.Effects.Application;
using Features.Buffs.Domain;

[RequireComponent(typeof(IStatsOwner))]
[RequireComponent(typeof(IBuffSource))]
public sealed class TurretBehaviour : NetworkBehaviour
{
    [Header("Refs")]
    public Transform turretHead;
    public Transform muzzlePoint;
    public LineRenderer laser;

    [Header("Config")]
    public float baseRange = 15f;
    public float lifetime = 25f;
    public LayerMask targetMask;

    private IStatsOwner statsOwner;
    private IStatsFacade stats;
    private IBuffSource source;

    private Transform target;
    private float fireTimer;
    private float retargetTimer;

    private readonly float sphereHeight = 0.8f;

    private void Awake()
    {
        statsOwner = GetComponent<IStatsOwner>();
        stats = statsOwner?.Facade;
        source = GetComponent<IBuffSource>();

        if (targetMask == 0)
            targetMask = LayerMask.GetMask("Enemy");
    }

    private void Start()
    {
        SetupLaser();
        StartCoroutine(LifeTimer());
    }

    private IEnumerator LifeTimer()
    {
        yield return new WaitForSeconds(lifetime);

        if (IsServerInitialized)
            GetComponent<NetworkObject>().Despawn();
    }

    private void Update()
    {
        if (!IsServerInitialized || statsOwner == null || !statsOwner.IsReady)
            return;

        TickCombat();
    }

    private void TickCombat()
    {
        retargetTimer -= Time.deltaTime;
        fireTimer -= Time.deltaTime;

        if (retargetTimer <= 0f)
        {
            AcquireTarget();
            retargetTimer = 0.15f;
        }

        if (target != null)
        {
            RotateToTarget();
            FireIfPossible();
            UpdateLaser();
        }
        else
        {
            DisableLaser();
        }
    }

    private void AcquireTarget()
    {
        Vector3 center = transform.position + Vector3.up * sphereHeight;
        Collider[] hits = Physics.OverlapSphere(center, baseRange, targetMask);

        float best = float.MaxValue;
        target = null;

        foreach (var h in hits)
        {
            if (!h.TryGetComponent<IBuffTarget>(out var buffTarget))
                continue;

            if (!buffTarget.IsReady)
                continue;

            float d = (h.transform.position - center).sqrMagnitude;
            if (d < best)
            {
                best = d;
                target = h.transform;
            }
        }
    }

    private void RotateToTarget()
    {
        if (!turretHead || target == null || stats?.Movement == null)
            return;

        Vector3 dir = target.position - turretHead.position;
        dir.y = 0f;

        float speed = stats.Movement.RotationSpeed;

        turretHead.rotation = Quaternion.Slerp(
            turretHead.rotation,
            Quaternion.LookRotation(dir),
            speed * Time.deltaTime
        );
    }

    private float FireInterval
    {
        get
        {
            if (stats?.Combat is ITurretCombatStats tc && tc.FireRate > 0f)
                return Mathf.Max(0.02f, 1f / tc.FireRate);

            return 1f;
        }
    }

    private float DamagePerShot
    {
        get
        {
            if (stats?.Combat == null)
                return 0f;

            return stats.Combat.DamageMultiplier;
        }
    }

    private void FireIfPossible()
    {
        if (fireTimer > 0f || target == null)
            return;

        if (!target.TryGetComponent<IBuffTarget>(out var buffTarget))
            return;

        var ctx = new EffectContext(
            source,
            new[] { buffTarget },
            muzzlePoint.position,
            (target.position - muzzlePoint.position).normalized
        );

        var effect = new DealDamageEffect(DamagePerShot, DamageType.Generic);
        effect.Apply(ctx);

        fireTimer = FireInterval;
    }

    private void SetupLaser()
    {
        if (!laser) return;

        laser.enabled = false;
        laser.startWidth = 0.05f;
        laser.endWidth = 0.05f;
    }

    private void UpdateLaser()
    {
        if (!laser || target == null || !muzzlePoint)
            return;

        laser.enabled = true;
        laser.SetPosition(0, muzzlePoint.position);
        laser.SetPosition(1, target.position);
    }

    private void DisableLaser()
    {
        if (laser)
            laser.enabled = false;
    }
}
