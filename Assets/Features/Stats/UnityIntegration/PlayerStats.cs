using UnityEngine;
using Features.Stats.Domain;
using Features.Stats.Adapter;


namespace Features.Stats.UnityIntegration
{
    /// <summary>
    /// Инициализирует Stats для игрока.
    /// Вызывается ТОЛЬКО из NetworkPlayer.OnStartClient() для локального игрока.
    /// </summary>
    [DefaultExecutionOrder(-400)]
    public class PlayerStats : MonoBehaviour
    {
        public IStatsFacade Facade { get; private set; }
        public StatsFacadeAdapter Adapter { get; private set; }

        public bool IsReady { get; private set; }

        public static event System.Action<PlayerStats> OnStatsReady;

        /// <summary>
        /// Инициализирует статы игрока.
        /// ВАЖНО: Должна быть вызвана из NetworkPlayer.OnStartClient()!
        /// </summary>
        public void Init()
        {
            Debug.Log("[PlayerStats] Init() called", this);

            if (IsReady)
            {
                Debug.LogWarning("[PlayerStats] Already initialized, skipping", this);
                return;
            }

            Facade = new StatsFacade(isTurret: false);
            Debug.Log("[PlayerStats] StatsFacade created", this);

            Adapter = GetComponent<StatsFacadeAdapter>();
            if (Adapter == null)
            {
                Debug.Log("[PlayerStats] StatsFacadeAdapter not found, adding component", this);
                Adapter = gameObject.AddComponent<StatsFacadeAdapter>();
            }

            Adapter.Init(Facade);
            Debug.Log("[PlayerStats] StatsFacadeAdapter initialized", this);

            IsReady = true;
            Debug.Log("[PlayerStats] PlayerStats is READY ✅", this);

            OnStatsReady?.Invoke(this);
        }

        public void ResetToDefaults()
        {
            if (!IsReady)
            {
                Debug.LogError("[PlayerStats] ResetToDefaults called but not ready!", this);
                return;
            }

            // Сбрасываем в нейтральное состояние
            Facade.Health.ApplyBase(100);
            Facade.Health.ApplyRegenBase(5);
            Facade.Energy.ApplyBase(100, 5);
            Facade.Combat.ApplyBase(1f);
            Facade.Movement.ApplyBase(5f, 3.5f, 6.5f, 2.5f, 180f);
            Facade.Mining.ApplyBase(1f);

            Debug.Log("[PlayerStats] Reset to defaults", this);
        }

        /// <summary>
        /// Получить текущие статы (безопасно).
        /// </summary>
        public IStatsFacade GetFacade()
        {
            if (!IsReady)
            {
                Debug.LogError("[PlayerStats] GetFacade called but stats not ready!", this);
                return null;
            }
            return Facade;
        }
    }
}
