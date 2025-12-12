using UnityEngine;
using Features.Stats.Domain;
using Features.Stats.Adapter;

namespace Features.Stats.UnityIntegration
{
    [DefaultExecutionOrder(-400)]
    public class TurretStats : MonoBehaviour
    {
        [Header("Preset")]
        public TurretPresetSO preset;

        public IStatsFacade Facade { get; private set; }
        public StatsFacadeAdapter Adapter { get; private set; }

        private void Awake()
        {
            Facade = new StatsFacade(isTurret: true);

            Adapter = gameObject.AddComponent<StatsFacadeAdapter>();
            Adapter.Init(Facade);

            ApplyBaseStats();
        }

        private void ApplyBaseStats()
        {
            if (!preset)
            {
                Debug.LogError("[TurretStats] Missing preset asset on turret!", this);
                return;
            }

            // DAMAGE
            Facade.Combat.ApplyBase(preset.baseDamageMultiplier);

            // FIRERATE
            if (Facade.Combat is ITurretCombatStats tc)
                tc.ApplyFireRateBase(preset.baseFireRate);

            // HP
            Facade.Health.ApplyBase(preset.baseHp);
            Facade.Health.ApplyRegenBase(preset.baseRegen);

            // MOVEMENT / ROTATION SPEED
            // Теперь MovementStats принимает rotationSpeed как 5-й аргумент.
            Facade.Movement.ApplyBase(
                baseSpeed: 0,
                walk: 0,
                sprint: 0,
                crouch: 0,
                rotation: preset.rotationSpeed
            );

            Debug.Log("[TurretStats] Base stats applied.");
        }

        // ============================================================
        // FINAL VALUES — API для TurretBehaviour
        // ============================================================

        // DAMAGE
        public float FinalDamage =>
            Facade?.Combat?.DamageMultiplier ?? 0f;

        // ROTATION SPEED (ТЕПЕРЬ ПРАВИЛЬНО)
        public float FinalRotationSpeed =>
            Facade?.Movement?.RotationSpeed ?? 0f;

        // FIRE RATE
        public float FinalFireRate =>
            (Facade?.Combat is ITurretCombatStats tc)
                ? tc.FireRate
                : 0f;

        // HP
        public float CurrentHp =>
            Facade?.Health?.CurrentHp ?? 0f;

        public float MaxHp =>
            Facade?.Health?.MaxHp ?? 0f;

        // DEBUG INSPECTOR FIELDS
        public float Debug_HP => CurrentHp;
        public float Debug_MaxHP => MaxHp;

        public float Debug_DamageMultiplier => FinalDamage;

        public float Debug_RotationSpeed => FinalRotationSpeed;

        public float Debug_FireRate => FinalFireRate;
    }
}
