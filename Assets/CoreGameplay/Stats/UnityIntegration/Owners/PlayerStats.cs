using UnityEngine;
using FishNet.Object;
using Features.Stats.Domain;
using Features.Stats.Adapter;
using Features.Stats.Application;

namespace Features.Stats.UnityIntegration
{
    [DefaultExecutionOrder(-400)]
    [RequireComponent(typeof(ServerGamePhase))]
    [RequireComponent(typeof(StatsBuffTarget))]
    public sealed class PlayerStats : StatsOwnerBase
    {
        public StatsFacadeAdapter Adapter { get; private set; }

        private ServerGamePhase phase;

        // =====================================================
        // SERVER
        // =====================================================

        protected override void InitStats()
        {
            base.InitStats();

            phase = GetComponent<ServerGamePhase>();

            ApplyClassDefaults();

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
                Facade.Combat.ApplyBase(
                    damageMultiplier: 1f,
                    fireRate: 6f,
                    spread: 2f,
                    aimSpread: 0.5f,
                    recoil: 1f,
                    range: 100f,
                    magazineSize: 30
                );
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

        // =====================================================
        // CLIENT (VIEW ONLY)
        // =====================================================

        public override void OnStartClient()
        {
            base.OnStartClient();

            Adapter = GetComponent<StatsFacadeAdapter>();
            if (Adapter == null)
                Adapter = gameObject.AddComponent<StatsFacadeAdapter>();

            Debug.Log("[PlayerStats] CLIENT ready (view only)", this);
        }

        // =====================================================
        // SERVER ROLE API
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
                    preset.combat.baseDamageMultiplier,
                    preset.combat.baseFireRate, 
                    preset.combat.baseRange,
                    preset.combat.baseSpread,
                    preset.combat.baseAimSpread,
                    preset.combat.baseRecoil,
                    preset.combat.baseMagazineSize
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
