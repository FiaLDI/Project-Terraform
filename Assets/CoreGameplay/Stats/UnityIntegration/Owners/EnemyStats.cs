using UnityEngine;
using FishNet.Object;
using Features.Stats.Domain;
using Features.Stats.Application;
using Features.Enemy.Data;
using Features.Stats.UnityIntegration;

namespace Features.Enemy.UnityIntegration
{
    [DefaultExecutionOrder(-400)]
    public sealed class EnemyStats : StatsOwnerBase
    {
        [Header("Config")]
        [SerializeField] private EnemyConfigSO config;

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
            BindBuffTarget();
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
                Facade.Combat.ApplyBase(1f);
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
                Facade.Combat.ApplyBase(preset.combat.baseDamageMultiplier);
            }
        }

        private void BindBuffTarget()
        {
            var buffTarget = GetComponent<EnemyBuffTarget>();
            if (buffTarget != null)
                buffTarget.SetStats(Facade);
        }
    }
}
