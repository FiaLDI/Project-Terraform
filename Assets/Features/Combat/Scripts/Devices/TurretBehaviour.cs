using UnityEngine;
using System.Collections;
using Features.Combat.Domain;
using Features.Buffs.Domain;
using Features.Buffs.Application;
using Features.Buffs.UnityIntegration;

namespace Features.Combat.Devices
{
    public class TurretBehaviour : MonoBehaviour, IDamageable, IBuffTarget
    {
        // ============================
        // IBuffTarget
        // ============================
        public Transform Transform => transform;
        public GameObject GameObject => gameObject;
        public BuffSystem BuffSystem => buffSystem;
        public BuffService Buffs { get; private set; }

        private BuffSystem buffSystem;
        private Transform player;  // <--- PlayerPositionStore интеграция

        // ============================
        // Base stats
        // ============================
        [Header("Base Stats")]
        public float baseDamagePerSecond = 4f;
        public float baseRange = 15f;
        public float lifetime = 25f;
        public int baseHp = 150;

        public float DamagePerSecond => baseDamagePerSecond * damageMultiplier;
        public float Range => baseRange;
        public float FireInterval => baseFireInterval / Mathf.Max(0.1f, fireRateMultiplier);

        [HideInInspector] public float damageMultiplier = 1f;
        [HideInInspector] public float fireRateMultiplier = 1f;
        [HideInInspector] public float rotationSpeedMultiplier = 1f;

        [Header("Combat")]
        public float baseFireInterval = 0.1f;
        public float retargetInterval = 0.2f;
        public LayerMask targetMask;

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


        // ============================
        // INIT CALLED FROM ABILITY
        // ============================
        public void Init(GameObject owner, int hp, float dps, float range, float life)
        {
            float globalBonus = GlobalBuffSystem.I != null
                ? GlobalBuffSystem.I.GetValue("turret_hp")
                : 0f;

            baseHp = Mathf.RoundToInt(hp * (1f + globalBonus / 100f));
            baseDamagePerSecond = dps;
            baseRange = range;
            lifetime = life;
        }


        // ============================
        // PLAYER FINDING
        // ============================
        private void TryFindPlayer()
        {
            if (PlayerPositionStore.Player != null)
            {
                player = PlayerPositionStore.Player;
                return;
            }

            var p = FindAnyObjectByType<PlayerController>();
            if (p != null)
                player = p.transform;
        }


        // ============================
        // UNITY LIFECYCLE
        // ============================
        private void Awake()
        {
            buffSystem = GetComponent<BuffSystem>();

            if (buffSystem == null)
            {
                Debug.LogWarning($"[Turret] No BuffSystem found on {name}, buffs disabled.");
                Buffs = null;
            }

            TryFindPlayer(); // <-- интеграция
        }

        private void Start()
        {
            Buffs = buffSystem?.GetServiceSafe();

            float global = GlobalBuffSystem.I != null
                ? GlobalBuffSystem.I.GetValue("turret_hp")
                : 0f;

            baseHp = Mathf.RoundToInt(baseHp * (1f + global / 100f));
            currentHp = baseHp;

            SetupLaser();
            StartCoroutine(LifeTimer());
        }

        private void SetupLaser()
        {
            if (!laser) return;

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
            if (!player)
                TryFindPlayer();

            ApplyBuffs();
            TickCombat();
        }


        // ============================
        // BUFFS
        // ============================
        private void ApplyBuffs()
        {
            damageMultiplier = 1f;
            fireRateMultiplier = 1f;
            rotationSpeedMultiplier = 1f;

            if (BuffSystem == null || BuffSystem.Active == null)
                return;

            foreach (var inst in BuffSystem.Active)
            {
                var cfg = inst.Config;
                if (cfg == null) continue;

                switch (cfg.stat)
                {
                    case BuffStat.TurretDamage:
                        ApplyStat(ref damageMultiplier, cfg);
                        break;

                    case BuffStat.TurretFireRate:
                        ApplyStat(ref fireRateMultiplier, cfg);
                        break;

                    case BuffStat.TurretRotationSpeed:
                        ApplyStat(ref rotationSpeedMultiplier, cfg);
                        break;
                }
            }
        }

        private void ApplyStat(ref float stat, BuffSO cfg)
        {
            if (cfg.modType == BuffModType.Add) stat += cfg.value;
            else if (cfg.modType == BuffModType.Mult) stat *= cfg.value;
        }


        // ============================
        // COMBAT
        // ============================
        private void TickCombat()
        {
            retargetTimer -= Time.deltaTime;
            fireTimer -= Time.deltaTime;

            if (retargetTimer <= 0f)
            {
                AcquireTarget();
                retargetTimer = retargetInterval;
            }

            if (target)
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
            if (fireTimer > 0f || target == null) return;

            float dmg = DamagePerSecond * FireInterval;

            if (target.TryGetComponent<IDamageable>(out var d))
                d.TakeDamage(dmg, DamageType.Generic);

            fireTimer = FireInterval;
        }

        private void UpdateLaser()
        {
            if (!laser || !muzzlePoint || !target) return;

            laser.enabled = true;
            laser.SetPosition(0, muzzlePoint.position);

            Vector3 tp = target.position;
            if (target.TryGetComponent<Collider>(out var c))
                tp = c.bounds.center;

            laser.SetPosition(1, tp);
        }

        private void DisableLaser()
        {
            if (laser) laser.enabled = false;
        }


        // ============================
        // HP
        // ============================
        public void TakeDamage(float amount, DamageType type)
        {
            currentHp -= amount;
            if (currentHp <= 0f) Destroy(gameObject);
        }

        public void Heal(float amount)
        {
            currentHp = Mathf.Min(currentHp + amount, baseHp);
        }
    }
}
