using UnityEngine;
using Features.Enemy.Data;
using Features.Enemy.Domain;
using Features.Combat.Domain;
using Features.Combat.Application;
using Features.Combat.Actors;
using Features.Stats.Domain;
using System.Collections.Generic;

namespace Features.Enemy.UnityIntegration
{
    public sealed class EnemyActor : MonoBehaviour, IDamageable
    {
        [SerializeField] private EnemyConfigSO config;

        private CombatService combat;
        private IHealthStats health;

        private Dictionary<HitboxType, float> multipliers;

        // =====================================================
        // UNITY
        // =====================================================

        private void Awake()
        {
            combat = CombatServiceProvider.Service;

            InitHitboxMultipliers();
        }

        private void Start()
        {
            BindStats();
        }

        // =====================================================
        // INIT
        // =====================================================

        private void BindStats()
        {
            var enemyStats = GetComponent<EnemyStats>();
            if (enemyStats == null || !enemyStats.IsReady)
            {
                Debug.LogError("[EnemyActor] EnemyStats not ready", this);
                enabled = false;
                return;
            }

            health = enemyStats.Facade.Health;
        }

        private void InitHitboxMultipliers()
        {
            multipliers = new Dictionary<HitboxType, float>();

            if (config == null || config.hitboxes == null)
                return;

            foreach (var h in config.hitboxes)
                multipliers[h.type] = h.damageMultiplier;
        }

        // =====================================================
        // HITBOX ENTRY POINT
        // =====================================================

        public void OnHitboxHit(HitInfo hit, HitboxType box)
        {
            float mult = multipliers.TryGetValue(box, out var m) ? m : 1f;

            HitInfo modified = hit;
            modified.damage *= mult;

            combat.ApplyDamage(this, modified, new DamageModifiers());
        }

        // =====================================================
        // IDamageable (CombatService → Stats)
        // =====================================================

        public void TakeDamage(float amount, DamageType type)
        {
            if (health == null)
                return;

            health.Damage(amount);
        }

        public void Heal(float healAmount)
        {
            if (health == null)
                return;

            health.Recover(healAmount);
        }
    }
}
