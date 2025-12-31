using UnityEngine;
using FishNet.Object;
using Features.Stats.Domain;
using Features.Stats.Adapter;

namespace Features.Stats.UnityIntegration
{
    /// <summary>
    /// Server-authoritative статы турели.
    /// </summary>
    [DefaultExecutionOrder(-400)]
    public sealed class TurretStats : NetworkBehaviour
    {
        [Header("Preset")]
        [SerializeField] private TurretPresetSO preset;

        public IStatsFacade Facade { get; private set; }
        public StatsFacadeAdapter Adapter { get; private set; }

        public bool IsReady { get; private set; }

        // =========================
        // SERVER
        // =========================

        public override void OnStartServer()
        {
            base.OnStartServer();
            InitServer();
        }

        private void InitServer()
        {
            if (IsReady)
                return;

            Facade = new StatsFacade(isTurret: true);

            ApplyBaseStats();

            IsReady = true;

            Debug.Log("[TurretStats] SERVER ready", this);
        }

        // =========================
        // CLIENT
        // =========================

        public override void OnStartClient()
        {
            base.OnStartClient();
            InitClient();
        }

        private void InitClient()
        {
            Adapter = GetComponent<StatsFacadeAdapter>();
            if (Adapter == null)
                Adapter = gameObject.AddComponent<StatsFacadeAdapter>();

            Debug.Log("[TurretStats] CLIENT ready (view only)", this);
        }

        // =========================
        // BASE STATS (SERVER ONLY)
        // =========================

        private void ApplyBaseStats()
        {
            if (!preset)
            {
                Debug.LogError("[TurretStats] Missing preset asset!", this);
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

            // MOVEMENT / ROTATION
            Facade.Movement.ApplyBase(
                baseSpeed: 0,
                walk: 0,
                sprint: 0,
                crouch: 0,
                rotation: preset.rotationSpeed
            );

            Debug.Log("[TurretStats] Base stats applied (SERVER)");
        }

        // =========================
        // SAFE ACCESS (SERVER)
        // =========================

        public IStatsFacade GetFacadeSafe()
        {
            if (!IsReady)
            {
                Debug.LogError("[TurretStats] GetFacadeSafe called before ready!", this);
                return null;
            }

            return Facade;
        }

        public float FinalDamage =>
            Facade?.Combat?.DamageMultiplier ?? 0f;

        // ROTATION SPEED
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
    }
}
