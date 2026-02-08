using UnityEngine;
using System.Collections;
using Features.Combat.Domain;
using Features.Stats.Domain;
using FishNet.Object;

public class TurretBehaviour : NetworkBehaviour, IDamageable
{
    [Header("Refs")]
    public Transform turretHead;
    public Transform muzzlePoint;
    public LineRenderer laser;

    [Header("Config")]
    public float baseRange = 15f;
    public float lifetime = 25f;
    public LayerMask targetMask;

    // =========================
    // STATS
    // =========================
    private IStatsOwner statsOwner;
    private IStatsFacade stats;

    // =========================
    // STATE
    // =========================
    private Transform target;
    private float fireTimer;
    private float retargetTimer;

    private readonly float sphereHeight = 0.8f;

    // =========================
    // UNITY
    // =========================

    private void Awake()
    {
        statsOwner = GetComponent<IStatsOwner>();
        stats = statsOwner?.Facade;

        if (statsOwner == null)
        {
            Debug.LogError("[TurretBehaviour] IStatsOwner not found", this);
            enabled = false;
            return;
        }

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

    // =========================
    // COMBAT
    // =========================

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
            if (!h.TryGetComponent<IDamageable>(out _))
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

    // =========================
    // FIRING
    // =========================

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

            return stats.Combat.DamageMultiplier * FireInterval;
        }
    }

    private void FireIfPossible()
    {
        if (fireTimer > 0f || target == null)
            return;

        if (target.TryGetComponent<IDamageable>(out var d))
            d.TakeDamage(DamagePerShot, DamageType.Generic);

        fireTimer = FireInterval;
    }

    // =========================
    // DAMAGE
    // =========================

    public void TakeDamage(float amount, DamageType type)
    {
        if (stats?.Health == null)
            return;

        stats.Health.Damage(amount);

        if (stats.Health.CurrentHp <= 0f && IsServerInitialized)
            GetComponent<NetworkObject>().Despawn();
    }

    public void Heal(float amount)
    {
        stats?.Health?.Heal(amount);
    }

    // =========================
    // LASER
    // =========================

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

    // =========================
    // NETWORKED DESTROY
    // =========================

    public void ScheduleDestruction(float delay)
    {
        if (IsServerInitialized)
            RpcDestroyAfterDelay(delay);
    }

    [ObserversRpc]
    private void RpcDestroyAfterDelay(float delay)
    {
        StartCoroutine(DestroyCoroutine(delay));
    }

    private IEnumerator DestroyCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (IsServerInitialized)
            GetComponent<NetworkObject>().Despawn();
    }
}
