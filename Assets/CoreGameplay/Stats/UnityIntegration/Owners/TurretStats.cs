using UnityEngine;
using Features.Stats.Domain;
using Features.Stats.Adapter;
using Features.Buffs.Domain;

namespace Features.Stats.UnityIntegration
{
    [DefaultExecutionOrder(-400)]
    [RequireComponent(typeof(StatsBuffTarget))]
    public sealed class TurretStats : StatsOwnerBase
    {
        [Header("Preset")]
        [SerializeField] private TurretPresetSO preset;

        private IBuffSource owner;

        public StatsFacadeAdapter Adapter { get; private set; }

        public void InitOwner(IBuffSource ownerSource)
        {
            owner = ownerSource;
        }

        // =========================
        // SERVER
        // =========================

        protected override void InitStats()
        {
            base.InitStats();
            ApplyBaseStats();
        }

        private void ApplyBaseStats()
        {
            if (preset == null)
            {
                Debug.LogError("[TurretStats] Missing TurretPresetSO", this);
                return;
            }

            if (Facade.Combat != null)
            {
                Facade.Combat.ApplyBase(preset.baseDamageMultiplier);

                if (Facade.Combat is ITurretCombatStats tc)
                    tc.ApplyFireRateBase(preset.baseFireRate);
            }

            if (Facade.Health != null)
            {
                Facade.Health.ApplyBase(preset.baseHp);
                Facade.Health.ApplyRegenBase(preset.baseRegen);
            }

            if (Facade.Movement != null)
            {
                Facade.Movement.ApplyBase(
                    0f, 0f, 0f, 0f,
                    preset.rotationSpeed
                );
            }

            Debug.Log("[TurretStats] Base stats applied (SERVER)", this);
        }

        // =========================
        // OWNER ACCESS
        // =========================

        public IBuffSource GetOwnerSource() => owner;

        // =========================
        // CLIENT
        // =========================

        public override void OnStartClient()
        {
            base.OnStartClient();

            Adapter = GetComponent<StatsFacadeAdapter>();
            if (Adapter == null)
                Adapter = gameObject.AddComponent<StatsFacadeAdapter>();
        }
    }
}
