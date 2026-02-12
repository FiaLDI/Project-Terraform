using UnityEngine;
using Features.Stats.Domain;
using Features.Enemy.Data;

namespace Features.Stats.UnityIntegration
{
    [DefaultExecutionOrder(-400)]
    [RequireComponent(typeof(StatsBuffTarget))]
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
    }
}
