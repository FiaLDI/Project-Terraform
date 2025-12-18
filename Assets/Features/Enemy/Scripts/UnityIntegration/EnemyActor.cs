using UnityEngine;
using Features.Enemy.Data;
using Features.Enemy.Domain;
using Features.Combat.Domain;
using Features.Combat.Application;
using Features.Combat.Actors;
using System.Collections.Generic;

namespace Features.Enemy.UnityIntegration
{
    public class EnemyActor : MonoBehaviour, IDamageable
    {
        [SerializeField] private EnemyConfigSO config;

        private HealthComponent health;
        private CombatService combat;

        private Dictionary<HitboxType, float> multipliers;

        private void Awake()
        {
            combat = CombatServiceProvider.Service;

            health = gameObject.AddComponent<HealthComponent>();
            health.maxHp = config.baseMaxHealth;
            health.currentHp = config.baseMaxHealth;

            multipliers = new Dictionary<HitboxType, float>();
            foreach (var h in config.hitboxes)
                multipliers[h.type] = h.damageMultiplier;
        }

        public void OnHitboxHit(HitInfo hit, HitboxType box)
        {
            float mult = multipliers.ContainsKey(box) ? multipliers[box] : 1f;

            HitInfo modified = hit;
            modified.damage *= mult;

            // передаём урон через CombatService
            combat.ApplyDamage(this, modified, new DamageModifiers());
        }

        // ===== IDamageable =====
        public void TakeDamage(float amount, DamageType type)
        {
            health.TakeDamage(amount, type);
        }

        public void Heal(float healAmount)
        {
            health.Heal(healAmount);
        }
    }
}
