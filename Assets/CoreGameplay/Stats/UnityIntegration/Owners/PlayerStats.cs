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
        [Header("Defaults")]
        [SerializeField] private StatsPresetSO defaultPreset;

        public StatsFacadeAdapter Adapter { get; private set; }

        private ServerGamePhase phase;
        private int level = 1;
        private float appliedHpMultiplier = 1f;
        private float appliedDamageMultiplier = 1f;
        private float appliedEnergyMultiplier = 1f;
        public int Level => level;

        protected override void InitStats()
        {
            base.InitStats();

            phase = GetComponent<ServerGamePhase>();
            Adapter = GetComponent<StatsFacadeAdapter>();

            ApplyDefaultPreset();
            InitServerAdapters();

            Debug.Log("[PlayerStats] SERVER ready -> StatsReady", this);
            phase.Reach(GamePhase.StatsReady);
        }

        private void ApplyDefaultPreset()
        {
            if (defaultPreset != null)
            {
                ApplyPresetValues(defaultPreset);
                return;
            }

            ApplyLegacyFallbackDefaults();
        }

        private void ApplyLegacyFallbackDefaults()
        {
            if (Facade.Health != null)
            {
                Facade.Health.ApplyBase(120f);
                Facade.Health.ApplyRegenBase(5f);
            }

            if (Facade.Energy != null)
                Facade.Energy.ApplyBase(150f, 8f);

            if (Facade.Combat != null)
            {
                Facade.Combat.ApplyBase(
                    baseDamage: 1f,
                    fireRate: 6f,
                    spread: 2f,
                    aimSpread: 0.5f,
                    range: 100f,
                    recoil: 1f,
                    magazineSize: 30,
                    critChance: 0.2f,
                    critMultiplier: 2f,
                    penetration: 0f
                );
            }

            if (Facade.Movement != null)
            {
                Facade.Movement.ApplyBase(
                    baseSpeed: 0f,
                    walk: 5f,
                    sprint: 8f,
                    crouch: 3.5f,
                    rotation: 180f,
                    gravity: -40f,
                    jumpHeight: 1.2f
                );
            }

            if (Facade.Mining != null)
                Facade.Mining.ApplyBase(1f);

            if (Facade.Protect != null)
            {
                Facade.Protect.ApplyBase(
                    genericResistance: 0f,
                    explosionResistance: 0f,
                    energyResistance: 0f,
                    miningResistance: 0f,
                    meleeResistance: 0f,
                    fireResistance: 0f,
                    electricResistance: 0f,
                    poisonResistance: 0f,
                    frostResistance: 0f,
                    acidResistance: 0f
                );
            }
        }

        private void ApplyPresetValues(StatsPresetSO preset)
        {
            if (preset == null)
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
                    baseDamage: preset.combat.baseDamageMultiplier,
                    fireRate: preset.combat.baseFireRate,
                    spread: preset.combat.baseSpread,
                    aimSpread: preset.combat.baseAimSpread,
                    range: preset.combat.baseRange,
                    recoil: preset.combat.baseRecoil,
                    magazineSize: preset.combat.baseMagazineSize,
                    critChance: 0.2f,
                    critMultiplier: 2f,
                    penetration: 0f
                );
            }

            if (Facade.Movement != null)
            {
                Facade.Movement.ApplyBase(
                    preset.movement.baseSpeed,
                    preset.movement.walkSpeed,
                    preset.movement.sprintSpeed,
                    preset.movement.crouchSpeed,
                    preset.movement.rotationSpeed,
                    preset.movement.gravity,
                    preset.movement.jumpHeight
                );
            }

            if (Facade.Mining != null)
                Facade.Mining.ApplyBase(preset.mining.baseMining);

            if (Facade.Protect != null)
            {
                Facade.Protect.ApplyBase(
                    preset.protect.generic,
                    preset.protect.explosion,
                    preset.protect.energy,
                    preset.protect.mining,
                    preset.protect.melee,
                    preset.protect.fire,
                    preset.protect.electric,
                    preset.protect.poison,
                    preset.protect.frost,
                    preset.protect.acid
                );
            }
        }

        private void InitServerAdapters()
        {
            if (Adapter == null || Facade == null)
                return;

            Adapter.CombatStats?.Init(Facade.Combat);
            Adapter.MovementStats?.Init(Facade.Movement);
            Adapter.MiningStats?.Init(Facade.Mining);
            Adapter.ProtectStats?.Init(Facade.Protect);
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            Adapter = GetComponent<StatsFacadeAdapter>();
            if (Adapter == null)
                Adapter = gameObject.AddComponent<StatsFacadeAdapter>();

            Debug.Log("[PlayerStats] CLIENT ready (view only)", this);
        }

        [Server]
        public void SetLevel(int newLevel)
        {
            if (!IsReady)
                return;

            level = Mathf.Max(1, newLevel);
            ApplyLevelScaling();

            Debug.Log($"[PlayerStats] Level set to {level}", this);
        }

        [Server]
        private void ApplyLevelScaling()
        {
            bool shouldRefillHp =
                Facade.Health != null &&
                Facade.Health.CurrentHp >= Facade.Health.MaxHp - 0.01f;

            bool shouldRefillEnergy =
                Facade.Energy != null &&
                Facade.Energy.CurrentEnergy >= Facade.Energy.MaxEnergy - 0.01f;

            RemoveLevelScaling();

            float hpMultiplier = 1f + (level - 1) * 0.05f;
            float damageMultiplier = 1f + (level - 1) * 0.03f;
            float energyMultiplier = 1f + (level - 1) * 0.04f;

            if (Facade.Health != null)
            {
                Facade.TryMultiply(StatKeys.MaxHp, hpMultiplier);
                appliedHpMultiplier = hpMultiplier;
            }

            if (Facade.Combat != null)
            {
                Facade.TryMultiply(StatKeys.DamageMultiplier, damageMultiplier);
                appliedDamageMultiplier = damageMultiplier;
            }

            if (Facade.Energy != null)
            {
                Facade.TryMultiply(StatKeys.MaxEnergy, energyMultiplier);
                appliedEnergyMultiplier = energyMultiplier;
            }

            if (shouldRefillHp && Facade.Health != null)
                Facade.Health.SetCurrentHp(Facade.Health.MaxHp);

            if (shouldRefillEnergy && Facade.Energy != null)
                Facade.Energy.SetCurrentEnergy(Facade.Energy.MaxEnergy);
        }

        [Server]
        private void RemoveLevelScaling()
        {
            if (Facade.Health != null && !Mathf.Approximately(appliedHpMultiplier, 1f))
                Facade.TryMultiply(StatKeys.MaxHp, 1f / appliedHpMultiplier);

            if (Facade.Combat != null && !Mathf.Approximately(appliedDamageMultiplier, 1f))
                Facade.TryMultiply(StatKeys.DamageMultiplier, 1f / appliedDamageMultiplier);

            if (Facade.Energy != null && !Mathf.Approximately(appliedEnergyMultiplier, 1f))
                Facade.TryMultiply(StatKeys.MaxEnergy, 1f / appliedEnergyMultiplier);

            ResetLevelScalingTracking();
        }

        private void ResetLevelScalingTracking()
        {
            appliedHpMultiplier = 1f;
            appliedDamageMultiplier = 1f;
            appliedEnergyMultiplier = 1f;
        }

        [Server]
        public void ResetAndApplyDefaults()
        {
            if (!IsReady)
                return;

            Facade.ResetAll();
            ResetLevelScalingTracking();
            ApplyDefaultPreset();
            ApplyLevelScaling();
        }

        [Server]
        public void ApplyPreset(StatsPresetSO preset)
        {
            if (!IsReady || preset == null)
                return;

            RemoveLevelScaling();
            ApplyPresetValues(preset);
            ApplyLevelScaling();
        }
    }
}
