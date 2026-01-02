using UnityEngine;
using FishNet.Object;
using Features.Stats.Domain;
using Features.Stats.Adapter;
using Features.Stats.Application;

namespace Features.Stats.UnityIntegration
{
    [DefaultExecutionOrder(-400)]
    [RequireComponent(typeof(ServerGamePhase))]
    public sealed class PlayerStats : NetworkBehaviour
    {
        // =====================================================
        // PUBLIC
        // =====================================================

        public IStatsFacade Facade { get; private set; }
        public StatsFacadeAdapter Adapter { get; private set; }
        public bool IsReady { get; private set; }

        private ServerGamePhase phase;

        // =====================================================
        // SERVER INIT
        // =====================================================

        public override void OnStartServer()
        {
            base.OnStartServer();
            phase = GetComponent<ServerGamePhase>();
            InitServer();
        }

        private void InitServer()
        {
            if (IsReady)
                return;

            // 1️⃣ Facade
            Facade = new StatsFacade(isTurret: false);

            // 2️⃣ Reset + defaults
            Facade.ResetAll();
            ApplyClassDefaults();

            // 3️⃣ Bind BuffTarget
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

            IsReady = true;

            Debug.Log("[PlayerStats] SERVER ready → StatsReady", this);

            // 🔥 ЕДИНСТВЕННЫЙ сигнал наружу
            phase.Reach(GamePhase.StatsReady);
        }

        // =====================================================
        // CLIENT INIT (VIEW ONLY)
        // =====================================================

        public override void OnStartClient()
        {
            base.OnStartClient();

            Adapter = GetComponent<StatsFacadeAdapter>();
            if (Adapter == null)
                Adapter = gameObject.AddComponent<StatsFacadeAdapter>();

            Debug.Log("[PlayerStats] CLIENT ready (view)", this);
        }

        // =====================================================
        // DEFAULTS (SERVER ONLY)
        // =====================================================

        private void ApplyClassDefaults()
        {
            Facade.Health.ApplyBase(120f);
            Facade.Health.ApplyRegenBase(5f);

            Facade.Energy.ApplyBase(150f, 8f);

            Facade.Combat.ApplyBase(1f);

            Facade.Movement.ApplyBase(
                baseSpeed: 0f,
                walk: 5f,
                sprint: 6.5f,
                crouch: 3.5f,
                rotation: 180f
            );

            Facade.Mining.ApplyBase(1f);
        }

        // =====================================================
        // SERVER API
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

            Facade.Health.ApplyBase(preset.health.baseHp);
            Facade.Health.ApplyRegenBase(preset.health.baseRegen);

            Facade.Energy.ApplyBase(
                preset.energy.baseMaxEnergy,
                preset.energy.baseRegen
            );

            Facade.Combat.ApplyBase(
                preset.combat.baseDamageMultiplier
            );

            Facade.Movement.ApplyBase(
                preset.movement.baseSpeed,
                preset.movement.walkSpeed,
                preset.movement.sprintSpeed,
                preset.movement.crouchSpeed,
                preset.movement.rotationSpeed
            );

            Facade.Mining.ApplyBase(
                preset.mining.baseMining
            );
        }

        // =====================================================
        // SAFE ACCESS
        // =====================================================

        public IStatsFacade GetFacadeSafe()
        {
            if (!IsReady)
            {
                Debug.LogError("[PlayerStats] Facade not ready", this);
                return null;
            }

            return Facade;
        }
    }
}
