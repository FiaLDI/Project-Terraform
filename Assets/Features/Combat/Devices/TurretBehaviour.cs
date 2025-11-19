using UnityEngine;
using System.Collections;

public class TurretBehaviour : MonoBehaviour, IDamageable, IBuffTarget
{
    // ============================================================
    // IBuffTarget implementation
    // ============================================================
    public Transform Transform => transform;
    public GameObject GameObject => gameObject;

    private BuffSystem buffSystem;
    public BuffSystem BuffSystem => buffSystem;

    // ============================================================
    // Base stats BEFORE buffs
    // ============================================================
    [Header("Base Stats")]
    public float baseDamagePerSecond = 4f;
    public float baseRange = 15f;
    public float lifetime = 25f;
    public int baseHp = 150;

    // Calculated stats (after buffs)
    public float DamagePerSecond => baseDamagePerSecond * damageMultiplier;
    public float Range => baseRange;
    public float FireInterval => baseFireInterval / Mathf.Max(0.1f, fireRateMultiplier);

    // Buff multipliers
    [HideInInspector] public float damageMultiplier = 1f;
    [HideInInspector] public float fireRateMultiplier = 1f;
    [HideInInspector] public float rotationSpeedMultiplier = 1f;

    // Passive global bonus
    public static float GlobalHpBonusPercent = 0f;

    // ============================================================
    // Combat settings
    // ============================================================
    [Header("Combat")]
    public float baseFireInterval = 0.1f;
    public float retargetInterval = 0.2f;
    public LayerMask targetMask;

    // ============================================================
    // Components
    // ============================================================
    [Header("Objects")]
    public Transform turretHead;
    public Transform muzzlePoint;

    [Header("Laser")]
    public LineRenderer laser;
    public Color laserColor = Color.red;
    public float laserWidth = 0.04f;

    private float currentHp;
    private float fireTimer;
    private float retargetTimer;
    private Transform target;

    private float sphereHeight = 0.8f;
    private Vector3 lastSphereCenter;

    // ============================================================
    // INIT (called by ability)
    // ============================================================
    public void Init(GameObject owner, int hp, float dps, float range, float life)
    {
        float globalBonus = GlobalBuffSystem.I.GetValue("turret_hp");

        baseHp = Mathf.RoundToInt(hp * (1f + globalBonus / 100f));
        baseDamagePerSecond = dps;
        baseRange = range;
        lifetime = life;
    }


    // ============================================================
    // UNITY LIFECYCLE
    // ============================================================
    private void Awake()
    {
        buffSystem = GetComponent<BuffSystem>();

        if (buffSystem == null)
            Debug.LogError($"Turret {name} has no BuffSystem!");
    }

    private void Start()
    {
        float globalBonus = GlobalBuffSystem.I.GetValue("turret_hp");
        baseHp = Mathf.RoundToInt(baseHp * (1f + globalBonus / 100f));

        currentHp = baseHp;

        SetupLaser();
        StartCoroutine(LifeTimer());
    }

    private void SetupLaser()
    {
        if (laser == null) return;

        laser.enabled = false;
        laser.startWidth = laserWidth;
        laser.endWidth = laserWidth;
        laser.startColor = laserColor;
        laser.endColor = laserColor;
    }

    private IEnumerator LifeTimer()
    {
        yield return new WaitForSeconds(lifetime);
        Destroy(gameObject);
    }

    private void Update()
    {
        UpdateBuffedStats();
        TickCombat();
    }

    // ============================================================
    // APPLY BUFFS TO TURRET
    // ============================================================
    private void UpdateBuffedStats()
    {
        // Reset multipliers before reapplying
        damageMultiplier = 1f;
        fireRateMultiplier = 1f;
        rotationSpeedMultiplier = 1f;

        foreach (var buff in buffSystem.Active)
        {
            if (buff.Config is DamageBoostBuffSO dmg)
                damageMultiplier *= dmg.multiplier;

            if (buff.Config is FireRateBuffSO fr)
                fireRateMultiplier *= fr.multiplier;

            if (buff.Config is MoveSpeedBuffSO rot)
                rotationSpeedMultiplier *= rot.speedMultiplier;
        }
    }

    // ============================================================
    // COMBAT LOGIC
    // ============================================================
    private void TickCombat()
    {
        retargetTimer -= Time.deltaTime;
        fireTimer -= Time.deltaTime;

        if (retargetTimer <= 0f)
        {
            AcquireTarget();
            retargetTimer = retargetInterval;
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
        lastSphereCenter = center;

        Collider[] hits = Physics.OverlapSphere(center, Range, targetMask);

        float bestDist = float.MaxValue;
        target = null;

        foreach (var h in hits)
        {
            float dist = (h.transform.position - center).sqrMagnitude;
            if (dist < bestDist)
            {
                bestDist = dist;
                target = h.transform;
            }
        }
    }

    private void RotateToTarget()
    {
        if (!turretHead || !target) return;

        Vector3 dir = target.position - turretHead.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.01f) return;

        Quaternion desired = Quaternion.LookRotation(dir);
        float speed = 10f * rotationSpeedMultiplier;

        turretHead.rotation = Quaternion.Slerp(
            turretHead.rotation,
            desired,
            speed * Time.deltaTime
        );
    }

    private void FireIfPossible()
    {
        if (fireTimer > 0f) return;
        if (target == null) return;

        float damage = DamagePerSecond * FireInterval;

        IDamageable dmg = 
            target.GetComponentInParent<IDamageable>() ??
            target.GetComponentInChildren<IDamageable>();

        dmg?.TakeDamage(damage, DamageType.Generic);

        fireTimer = FireInterval;
    }

    // ============================================================
    // LASER
    // ============================================================
    private void UpdateLaser()
    {
        if (!laser || !muzzlePoint || !target) return;

        laser.enabled = true;
        laser.SetPosition(0, muzzlePoint.position);

        Vector3 targetPos = target.position;

        if (target.TryGetComponent<Collider>(out var col))
            targetPos = col.bounds.center;

        laser.SetPosition(1, targetPos);
    }

    private void DisableLaser()
    {
        if (laser != null)
            laser.enabled = false;
    }

    // ============================================================
    // HP
    // ============================================================
    public void TakeDamage(float amount, DamageType type)
    {
        currentHp -= amount;
        if (currentHp <= 0f)
            Destroy(gameObject);
    }

    public void Heal(float amount)
    {
        currentHp = Mathf.Min(currentHp + amount, baseHp);
    }

    // ============================================================
    // GIZMOS
    // ============================================================
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Vector3 center = Application.isPlaying
            ? lastSphereCenter
            : transform.position + Vector3.up * sphereHeight;

        Gizmos.DrawWireSphere(center, baseRange);
    }
}
