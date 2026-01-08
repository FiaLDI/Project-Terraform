using FishNet.Object;
using Features.Stats.Domain;
using Features.Stats.Application;
using Features.Enemy.Data;
using UnityEngine;

namespace Features.Enemy.UnityIntegration
{
    [DefaultExecutionOrder(-400)]
    public sealed class EnemyStats : NetworkBehaviour
    {
        [Header("Config")]
        [SerializeField] private EnemyConfigSO config;

        public IStatsFacade Facade { get; private set; }
        public bool IsReady { get; private set; }

        public override void OnStartServer()
        {
            base.OnStartServer();
            InitServer();
        }

        private void InitServer()
        {
            if (IsReady)
                return;

            if (config == null)
            {
                Debug.LogError("[EnemyStats] EnemyConfigSO not assigned", this);
                return;
            }

            Facade = new StatsFacade(isTurret: true);

            Facade.ResetAll();
            ApplyDefaultsFromConfig();

            var buffTarget = GetComponent<EnemyBuffTarget>();
            if (buffTarget != null)
                buffTarget.SetStats(Facade);

            IsReady = true;
        }

        private void ApplyDefaultsFromConfig()
        {
            if (config.statsPreset != null)
            {
                ApplyPreset(config.statsPreset);
                return;
            }

            Facade.Health.ApplyBase(100f);
            Facade.Health.ApplyRegenBase(0f);
            Facade.Combat.ApplyBase(1f);
        }

        private void ApplyPreset(EnemyStatsPresetSO preset)
        {
            Facade.Health.ApplyBase(preset.health.baseHp);
            Facade.Health.ApplyRegenBase(preset.health.baseRegen);
            Facade.Combat.ApplyBase(preset.combat.baseDamageMultiplier);
        }
    }
}
