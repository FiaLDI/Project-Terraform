using UnityEngine;
using System.Collections;
using Features.Combat.Domain;
using Features.Stats.UnityIntegration;
using FishNet.Object;

public class TurretBehaviour : NetworkBehaviour, IDamageable
{
    public Transform turretHead;
    public Transform muzzlePoint;
    public LineRenderer laser;

    public float baseRange = 15f;
    public float lifetime = 25f;
    public LayerMask targetMask;

    private TurretStats stats;

    private Transform target;
    private float fireTimer;
    private float retargetTimer;

    private float sphereHeight = 0.8f;

    private void Awake()
    {
        stats = GetComponent<TurretStats>();

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
        Destroy(gameObject);
    }

    private void Update()
    {
        TickCombat();
    }

    private void TickCombat()
    {
        retargetTimer -= Time.deltaTime;
        fireTimer -= Time.deltaTime;

        if (retargetTimer <= 0)
        {
            AcquireTarget();
            retargetTimer = 0.15f;
        }

        if (target)
        {
            RotateToTarget();
            FireIfPossible();
            UpdateLaser();
        }
        else DisableLaser();
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
        if (!turretHead || !target) return;

        Vector3 dir = target.position - turretHead.position;
        dir.y = 0f;

        float speed = stats.FinalRotationSpeed;

        turretHead.rotation = Quaternion.Slerp(
            turretHead.rotation,
            Quaternion.LookRotation(dir),
            speed * Time.deltaTime
        );
    }

    private float FireInterval =>
        Mathf.Max(0.02f, 1f / stats.FinalFireRate);

    private float DamagePerShot =>
        stats.FinalDamage * FireInterval;

    private void FireIfPossible()
    {
        if (fireTimer > 0f || !target) return;

        if (target.TryGetComponent<IDamageable>(out var d))
            d.TakeDamage(DamagePerShot, DamageType.Generic);

        fireTimer = FireInterval;
    }

    public void TakeDamage(float amount, DamageType type)
    {
        stats.Facade.Health.Damage(amount);
        if (stats.Facade.Health.CurrentHp <= 0)
            Destroy(gameObject);
    }

    public void Heal(float amount) =>
        stats.Facade.Health.Heal(amount);

    private void SetupLaser()
    {
        if (!laser) return;

        laser.enabled = false;
        laser.startWidth = 0.05f;
        laser.endWidth = 0.05f;
    }

    private void UpdateLaser()
    {
        if (!laser || !target || !muzzlePoint) return;

        laser.enabled = true;
        laser.SetPosition(0, muzzlePoint.position);
        laser.SetPosition(1, target.position);
    }

    private void DisableLaser()
    {
        if (laser)
            laser.enabled = false;
    }

    /// <summary>
    /// 🎯 Синхронизированное уничтожение через сетевой таймер
    /// </summary>
    public void ScheduleDestruction(float delay)
    {
        if (IsServerInitialized)
        {
            RpcDestroyAfterDelay(delay);
        }
    }

    [ObserversRpc]
    private void RpcDestroyAfterDelay(float delay)
    {
        StartCoroutine(DestroyCoroutine(delay));
    }

    private System.Collections.IEnumerator DestroyCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (IsServerInitialized)
        {
            GetComponent<NetworkObject>().Despawn();
        }
    }
}
