using UnityEngine;
using FishNet.Object;
using Features.Stats.Domain;
using Features.Stats.Adapter;
using Features.Stats.Application;

namespace Features.Stats.UnityIntegration
{
    [DefaultExecutionOrder(-400)]
    [RequireComponent(typeof(ServerGamePhase))]
    public sealed class PlayerStats : StatsOwnerBase
    {
        public StatsFacadeAdapter Adapter { get; private set; }

        private ServerGamePhase phase;

        // =========================
        // SERVER
        // =========================

        protected override void InitStats()
        {
            base.InitStats();

            phase = GetComponent<ServerGamePhase>();

            ApplyClassDefaults();
            BindBuffTarget();

            Debug.Log("[PlayerStats] SERVER ready → StatsReady", this);
            phase.Reach(GamePhase.StatsReady);
        }

        private void ApplyClassDefaults()
        {
            if (Facade.Health != null)
            {
                Facade.Health.ApplyBase(120f);
                Facade.Health.ApplyRegenBase(5f);
            }

            if (Facade.Energy != null)
            {
                Facade.Energy.ApplyBase(150f, 8f);
            }

            if (Facade.Combat != null)
            {
                Facade.Combat.ApplyBase(1f);
            }

            if (Facade.Movement != null)
            {
                Facade.Movement.ApplyBase(
                    baseSpeed: 0f,
                    walk: 5f,
                    sprint: 6.5f,
                    crouch: 3.5f,
                    rotation: 180f
                );
            }

            if (Facade.Mining != null)
            {
                Facade.Mining.ApplyBase(1f);
            }
        }

        private void BindBuffTarget()
        {
            var buffTarget = GetComponent<PlayerBuffTarget>();
            if (buffTarget != null)
            {
                buffTarget.SetStats(Facade);
                Debug.Log("[PlayerStats] BuffTarget linked", this);
            }
            else
            {
                Debug.LogWarning("[PlayerStats] PlayerBuffTarget missing", this);
            }
        }

        // =========================
        // CLIENT (VIEW ONLY)
        // =========================

        public override void OnStartClient()
        {
            base.OnStartClient();

            Adapter = GetComponent<StatsFacadeAdapter>();
            if (Adapter == null)
                Adapter = gameObject.AddComponent<StatsFacadeAdapter>();

            Debug.Log("[PlayerStats] CLIENT ready (view)", this);
        }

        // =====================================================
        // SERVER API (ROLE-SPECIFIC)
        // =====================================================

        [Server]
        public void ResetAndApplyDefaults()
        {
            if (!IsReady)
                return;

            Facade.ResetAll();
            ApplyClassDefaults();
        }

        [Server]
        public void ApplyPreset(StatsPresetSO preset)
        {
            if (!IsReady || preset == null)
                return;

            if (Facade.Health != null)
            {
                Facade.Health.ApplyBase(preset.health.baseHp);
                Facade.Health.ApplyRegenBase(preset.health.baseRegen);
            }

            if (Facade.Energy != null)
            {
                Facade.Energy.ApplyBase(
                    preset.energy.baseMaxEnergy,
                    preset.energy.baseRegen
                );
            }

            if (Facade.Combat != null)
            {
                Facade.Combat.ApplyBase(
                    preset.combat.baseDamageMultiplier
                );
            }

            if (Facade.Movement != null)
            {
                Facade.Movement.ApplyBase(
                    preset.movement.baseSpeed,
                    preset.movement.walkSpeed,
                    preset.movement.sprintSpeed,
                    preset.movement.crouchSpeed,
                    preset.movement.rotationSpeed
                );
            }

            if (Facade.Mining != null)
            {
                Facade.Mining.ApplyBase(
                    preset.mining.baseMining
                );
            }
}
    }
}
