using UnityEngine;
using FishNet.Object;
using Features.Stats.Domain;
using Features.Stats.Adapter;
using Features.Stats.Application;

namespace Features.Stats.UnityIntegration
{
    /// <summary>
    /// ЕДИНСТВЕННАЯ точка инициализации статов игрока.
    ///
    /// SERVER:
    ///  - создаёт StatsFacade
    ///  - НЕ применяет класс
    ///  - предоставляет Reset + Defaults
    ///
    /// CLIENT:
    ///  - создаёт ТОЛЬКО view (Adapter)
    ///  - получает значения ТОЛЬКО через StatsNetSync
    /// </summary>
    [DefaultExecutionOrder(-400)]
    public sealed class PlayerStats : NetworkBehaviour
    {
        // =====================================================
        // PUBLIC API
        // =====================================================

        public IStatsFacade Facade { get; private set; }
        public StatsFacadeAdapter Adapter { get; private set; }

        public bool IsReady { get; private set; }

        /// <summary>
        /// Событие только для биндинга (НЕ для применения класса).
        /// </summary>
        public static event System.Action<PlayerStats> OnStatsReady;

        // =====================================================
        // SERVER INIT (AUTHORITATIVE)
        // =====================================================

        public override void OnStartServer()
        {
            base.OnStartServer();
            InitServer();
        }

        private void InitServer()
        {
            if (IsReady)
                return;

            // 1️⃣ Создаём фасад
            Facade = new StatsFacade(isTurret: false);

            // 2️⃣ СБРОС + БАЗОВЫЕ ЗНАЧЕНИЯ
            Facade.ResetAll();
            ApplyClassDefaults();

            var buffTarget = GetComponent<PlayerBuffTarget>();
            if (buffTarget != null)
            {
                buffTarget.SetStats(Facade);
                Debug.Log("[PlayerStats] BuffTarget linked with StatsFacade", this);
            }
            else
            {
                Debug.LogWarning("[PlayerStats] PlayerBuffTarget not found", this);
            }

            IsReady = true;

            Debug.Log("[PlayerStats] SERVER ready (StatsFacade created)", this);

            // ❗ ТОЛЬКО сигнал готовности
            OnStatsReady?.Invoke(this);
        }

        // =====================================================
        // CLIENT INIT (VIEW ONLY)
        // =====================================================

        public override void OnStartClient()
        {
            base.OnStartClient();
            InitClient();
        }

        private void InitClient()
        {
            // Клиент НЕ создаёт StatsFacade
            // Он только отображает значения, полученные по сети

            Adapter = GetComponent<StatsFacadeAdapter>();
            if (Adapter == null)
                Adapter = gameObject.AddComponent<StatsFacadeAdapter>();

            Debug.Log("[PlayerStats] CLIENT ready (view only)", this);
        }

        // =====================================================
        // BASE DEFAULTS (SERVER ONLY)
        // =====================================================

        /// <summary>
        /// Базовые значения игрока БЕЗ класса.
        /// Вызывается ТОЛЬКО сервером.
        /// </summary>
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

            Debug.Log("[PlayerStats] SERVER defaults applied", this);
        }

        // =====================================================
        // SERVER API
        // =====================================================

        /// <summary>
        /// Полный сброс + дефолты.
        /// ЕДИНСТВЕННАЯ точка для NetAdapter.
        /// </summary>
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
                Debug.LogError(
                    "[PlayerStats] GetFacadeSafe called before ready!",
                    this
                );
                return null;
            }

            return Facade;
        }
    }
}
