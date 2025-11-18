using UnityEngine;
using System.Collections;

public class TurretBehaviour : MonoBehaviour, IDamageable
{
    // ------------------ СТАТЫ ------------------
    [Header("Статы")]
    public float damagePerSecond = 4f;
    public float range = 15f;
    public float lifetime = 25f;
    public int maxHp = 150;

    // Бонус HP от пассивок (глобальный)
    public static float GlobalHpBonusPercent = 0f;


    // ------------------ БОЕВЫЕ ПАРАМЕТРЫ ------------------
    [Header("Боевые параметры")]
    public float retargetInterval = 0.2f;
    public float fireInterval = 0.1f;
    public LayerMask targetMask;


    // ------------------ ВРАЩЕНИЕ ------------------
    [Header("Ротация")]
    public Transform turretHead; // объект Rotator
    public Transform muzzlePoint; // точка выхода лазера


    // ------------------ ЛАЗЕР ------------------
    [Header("Лазер")]
    public LineRenderer laser;
    public Color laserColor = Color.red;
    public float laserWidth = 0.04f;


    // ------------------ ВНУТРЕННИЕ ------------------
    private float currentHp;
    private Transform target;

    private float retargetTimer;
    private float fireTimer;

    private float sphereHeight = 0.8f;
    private Vector3 lastSphereCenter;



    // ============================================================
    //                         START
    // ============================================================
    private void Start()
    {
        currentHp = maxHp;

        // Инициализация LineRenderer
        if (laser != null)
        {
            laser.enabled = false;
            laser.startWidth = laserWidth;
            laser.endWidth = laserWidth;
            laser.startColor = laserColor;
            laser.endColor = laserColor;
        }

        StartCoroutine(LifeTimer());
    }

    private IEnumerator LifeTimer()
    {
        yield return new WaitForSeconds(lifetime);
        Destroy(gameObject);
    }


    // ============================================================
    //                         INIT
    // ============================================================
    public void Init(GameObject owner, int hp, float dps, float range, float lifetime)
    {
        this.maxHp = Mathf.RoundToInt(hp * (1f + GlobalHpBonusPercent / 100f));
        this.damagePerSecond = dps;
        this.range = range;
        this.lifetime = lifetime;
    }


    // ============================================================
    //                          UPDATE
    // ============================================================
    private void Update()
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
            TryFire();
            UpdateLaser();
        }
        else
        {
            DisableLaser();
        }
    }


    // ============================================================
    //                    ПОИСК ЦЕЛИ
    // ============================================================
    private void AcquireTarget()
    {
        Vector3 center = transform.position + Vector3.up * sphereHeight;
        lastSphereCenter = center;

        Collider[] hits = Physics.OverlapSphere(center, range, targetMask);

        float closest = Mathf.Infinity;
        target = null;

        foreach (var h in hits)
        {
            float dist = Vector3.Distance(center, h.transform.position);
            if (dist < closest)
            {
                closest = dist;
                target = h.transform;
            }
        }
    }


    // ============================================================
    //                    ВРАЩЕНИЕ ТУРЕЛИ
    // ============================================================
    private void RotateToTarget()
    {
        if (turretHead == null || target == null)
            return;

        Vector3 dir = target.position - turretHead.position;
        dir.y = 0;

        if (dir.sqrMagnitude > 0.01f)
        {
            Quaternion rot = Quaternion.LookRotation(dir);
            turretHead.rotation = Quaternion.Slerp(
                turretHead.rotation,
                rot,
                10f * Time.deltaTime
            );
        }
    }


    // ============================================================
    //                      АТАКА / УРОН
    // ============================================================
    private void TryFire()
    {
        if (target == null) return;
        if (fireTimer > 0f) return;

        float dmg = damagePerSecond * fireInterval;

        IDamageable dmgTarget =
            target.GetComponentInParent<IDamageable>() ??
            target.GetComponentInChildren<IDamageable>();

        if (dmgTarget != null)
            dmgTarget.TakeDamage(dmg, DamageType.Generic);

        fireTimer = fireInterval;
    }


    // ============================================================
    //                       ЛАЗЕР
    // ============================================================
    private void UpdateLaser()
    {
        if (laser == null || muzzlePoint == null || target == null)
            return;

        laser.enabled = true;

        Vector3 start = muzzlePoint.position;

        // Центр коллайдера врага
        Vector3 end = target.position;
        var col = target.GetComponent<Collider>();
        if (col != null)
            end = col.bounds.center;

        laser.SetPosition(0, start);
        laser.SetPosition(1, end);
    }


    private void DisableLaser()
    {
        if (laser != null)
            laser.enabled = false;
    }


    // ============================================================
    //                 ПОЛУЧЕНИЕ УРОНА ТУРЕЛЬЮ
    // ============================================================
    public void TakeDamage(float amount, DamageType type)
    {
        currentHp -= amount;
        if (currentHp <= 0)
            Destroy(gameObject);
    }

    public void Heal(float amount)
    {
        currentHp = Mathf.Min(maxHp, currentHp + amount);
    }


    // ============================================================
    //                         GIZMOS
    // ============================================================
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Vector3 center = Application.isPlaying
            ? lastSphereCenter
            : transform.position + Vector3.up * sphereHeight;

        Gizmos.DrawWireSphere(center, range);
    }
}
