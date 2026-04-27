using UnityEngine;
using System.Collections;
using FishNet.Object;
using Features.Stats.Domain;
using Features.Effects.Domain;
using Features.Buffs.Domain;
using Features.Effects.Application;
using Features.Weapons.Domain;

[RequireComponent(typeof(IStatsOwner))]
[RequireComponent(typeof(IBuffSource))]
public sealed class TurretBehaviour : NetworkBehaviour
{
    [Header("Refs")]
    public Transform turretHead;
    public Transform muzzlePoint;
    public ProjectileConfig projectileConfig;

    [Header("Config")]
    public float baseRange = 15f;
    public float lifetime = 25f;
    public LayerMask targetMask;

    private IBuffSource source;
    private Transform target;
    private float fireTimer;
    private float retargetTimer;

    private readonly float sphereHeight = 0.8f;

    private IStatsOwner StatsOwner => GetComponent<IStatsOwner>();
    private IStatsFacade Stats => StatsOwner?.Facade;

    private void Awake()
    {
        source = GetComponent<IBuffSource>();

        if (targetMask == 0)
            targetMask = LayerMask.GetMask("Enemy");
    }

    private void Start()
    {
        if (IsServerInitialized)
            StartCoroutine(LifeTimer());
    }

    private IEnumerator LifeTimer()
    {
        yield return new WaitForSeconds(lifetime);

        if (IsServerInitialized)
            base.NetworkObject.Despawn();
    }

    private void Update()
    {
        if (IsServerInitialized)
        {
            var owner = StatsOwner;
            if (owner == null || !owner.IsReady || owner.Facade == null)
                return;

            TickCombat();
        }

        UpdateVisualsLocal();
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
            RotateToTargetServer();
            FireIfPossible();
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

    private Quaternion syncedHeadRotation;

    private void RotateToTargetServer()
    {
        if (!turretHead || target == null)
            return;

        var stats = Stats;
        if (stats?.Movement == null)
            return;

        Vector3 dir = target.position - turretHead.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.0001f)
            return;

        float speed = stats.Movement.RotationSpeed;

        Quaternion nextRotation = Quaternion.Slerp(
            turretHead.rotation,
            Quaternion.LookRotation(dir),
            speed * Time.deltaTime
        );

        turretHead.rotation = nextRotation;
        syncedHeadRotation = nextRotation;
        ObserversSetHeadRotation(nextRotation);
    }

    [ObserversRpc(BufferLast = true)]
    private void ObserversSetHeadRotation(Quaternion rotation)
    {
        syncedHeadRotation = rotation;
    }

    private void UpdateVisualsLocal()
    {
        if (turretHead != null && !IsServerInitialized)
        {
            turretHead.rotation = syncedHeadRotation;
        }
    }

    private float FireInterval
    {
        get
        {
            var stats = Stats;

            if (stats?.Combat is ITurretCombatStats tc && tc.FireRate > 0f)
                return Mathf.Max(0.02f, 1f / tc.FireRate);

            return 1f;
        }
    }

    private float DamagePerShot
    {
        get
        {
            var stats = Stats;
            return stats?.Combat?.FinalDamage ?? 0f;
        }
    }

    private void FireIfPossible()
    {
        if (fireTimer > 0f || target == null || muzzlePoint == null)
            return;

        Vector3 hitPoint = ResolveTargetPoint(target);
        Vector3 direction = (hitPoint - muzzlePoint.position).normalized;

        if (direction.sqrMagnitude < 0.0001f)
            return;

        if (projectileConfig != null)
        {
            FireProjectile(hitPoint, direction);
            fireTimer = FireInterval;
            return;
        }

        FireDirectDamage(hitPoint, direction);
        fireTimer = FireInterval;
    }

    private void FireProjectile(Vector3 hitPoint, Vector3 direction)
    {
        var ctx = new HitEffectContext(
            source,
            null,
            muzzlePoint.position,
            direction,
            hitPoint,
            -direction
        );

        new SpawnProjectileEffect(projectileConfig).Apply(ctx);

        if (ShouldSpawnClientProjectileVisual())
            ObserversPlayProjectileVisual(muzzlePoint.position, hitPoint);
    }

    private void FireDirectDamage(Vector3 hitPoint, Vector3 direction)
    {
        if (!target.TryGetComponent<IBuffTarget>(out var buffTarget))
            return;

        float damage = DamagePerShot;
        if (damage <= 0f)
            return;

        var ctx = new EffectContext(
            source,
            new[] { buffTarget },
            muzzlePoint.position,
            direction
        );

        var effect = new DealDamageEffect(damage, DamageType.Generic);
        effect.Apply(ctx);
    }

    private Vector3 ResolveTargetPoint(Transform targetTransform)
    {
        if (targetTransform.TryGetComponent<Collider>(out var collider))
            return collider.bounds.center;

        return targetTransform.position;
    }

    private bool ShouldSpawnClientProjectileVisual()
    {
        return projectileConfig != null &&
               !projectileConfig.useServerProjectile &&
               projectileConfig.clientProjectilePrefab != null;
    }

    [ObserversRpc]
    private void ObserversPlayProjectileVisual(Vector3 spawnPos, Vector3 hitPoint)
    {
        if (!ShouldSpawnClientProjectileVisual())
            return;

        if (ProjectilePool.Instance == null)
            return;

        Quaternion rotation = projectileConfig.visualType == ProjectileVisualType.Projectile
            ? Quaternion.LookRotation((hitPoint - spawnPos).normalized)
            : Quaternion.identity;

        var go = ProjectilePool.Instance.Get(
            projectileConfig.clientProjectilePrefab,
            spawnPos,
            rotation
        );

        var visual = go != null ? go.GetComponent<IProjectileVisual>() : null;
        if (visual != null)
            visual.Init(spawnPos, hitPoint, projectileConfig.lifetime);
    }
}
