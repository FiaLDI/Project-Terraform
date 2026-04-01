using Features.Buffs.Domain;
using Features.Enemy.Data;
using Features.Quests.Application;
using Features.Quests.Domain;
using FishNet;
using FishNet.Object;
using UnityEngine;

namespace Features.Stats.UnityIntegration
{
    [DefaultExecutionOrder(-400)]
    [RequireComponent(typeof(StatsBuffTarget))]
    public sealed class EnemyStats : StatsOwnerBase
    {
        [Header("Config")]
        [SerializeField] private EnemyConfigSO config;
        private IBuffSource lastAttacker;
        private bool isDead;

        // =========================
        // SERVER
        // =========================

        protected override void InitStats()
        {
            base.InitStats();

            if (config == null)
            {
                Debug.LogError("[EnemyStats] EnemyConfigSO not assigned", this);
                return;
            }

            ApplyDefaultsFromConfig();
        }

        private void ApplyDefaultsFromConfig()
        {
            if (config.statsPreset != null)
            {
                ApplyPreset(config.statsPreset);
                return;
            }

            if (Facade.Health != null)
            {
                Facade.Health.ApplyBase(100f);
                Facade.Health.ApplyRegenBase(0f);
            }

            if (Facade.Combat != null)
            {
                Facade.Combat.ApplyBase(
                    config.statsPreset.combat.baseDamageMultiplier,
                    fireRate: 6f,
                    spread: 2f,
                    aimSpread: 0.5f,
                    recoil: 1f,
                    range: 100f,
                    magazineSize: 30
                );
            }
        }

        private void ApplyPreset(EnemyStatsPresetSO preset)
        {
            if (Facade.Health != null)
            {
                Facade.Health.ApplyBase(preset.health.baseHp);
                Facade.Health.ApplyRegenBase(preset.health.baseRegen);
            }

            if (Facade.Combat != null)
            {
                Facade.Combat.ApplyBase(
                    config.statsPreset.combat.baseDamageMultiplier,
                    fireRate: 6f,
                    spread: 2f,
                    aimSpread: 0.5f,
                    recoil: 1f,
                    range: 100f,
                    magazineSize: 30
                );
            }
        }

        public void RegisterAttacker(IBuffSource attacker)
        {
            lastAttacker = attacker;
        }

        private void Update()
        {
            if (!IsServer)
                return;

            CheckDeath();
        }

        private void CheckDeath()
        {
            if (isDead)
                return;

            if (Facade.Health == null)
                return;

            if (Facade.Health.CurrentHp > 0)
                return;

            isDead = true;

            Debug.Log("[Enemy] Died");

            QuestEventBus.Publish(
                lastAttacker, 
                new EnemyKilledEvent(config.enemyId, lastAttacker)
            );

            if (InstanceFinder.IsServer)
            {
                var netObj = GetComponent<NetworkObject>();
                if (netObj != null)
                {
                    InstanceFinder.ServerManager.Despawn(netObj);
                }
                else
                {
                    Destroy(gameObject); // fallback
                }
            }
        }
    }
}
