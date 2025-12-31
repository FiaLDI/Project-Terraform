using UnityEngine;
using FishNet.Object;
using Features.Stats.Domain;
using Features.Stats.Adapter;

namespace Features.Stats.UnityIntegration
{
    /// <summary>
    /// ЕДИНСТВЕННАЯ точка инициализации статов игрока.
    /// SERVER:
    ///  - создаёт StatsFacade
    ///  - применяет класс / дефолты
    ///
    /// CLIENT:
    ///  - создаёт ТОЛЬКО адаптеры (view)
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

            Facade = new StatsFacade(isTurret: false);

            Facade.ResetAll();

            ApplyClassDefaults();

            IsReady = true;

            Debug.Log("[PlayerStats] SERVER ready (StatsFacade created)", this);
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
            // Он только отображает данные, пришедшие по сети

            Adapter = GetComponent<StatsFacadeAdapter>();
            if (Adapter == null)
            {
                Adapter = gameObject.AddComponent<StatsFacadeAdapter>();
            }

            Debug.Log("[PlayerStats] CLIENT ready (view only)", this);
        }

        // =====================================================
        // CLASS / DEFAULTS (SERVER ONLY)
        // =====================================================

        private void ApplyClassDefaults()
        {
            // ❗ ЭТО ТОЛЬКО ПРИМЕР
            // Здесь ты применяешь:
            // - класс игрока
            // - стартовые перки
            // - балансные значения

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

            Debug.Log("[PlayerStats] SERVER class defaults applied", this);
        }

        // =====================================================
        // SAFE ACCESS
        // =====================================================

        public IStatsFacade GetFacadeSafe()
        {
            if (!IsReady)
            {
                Debug.LogError("[PlayerStats] GetFacadeSafe called before ready!", this);
                return null;
            }

            return Facade;
        }
    }
}
