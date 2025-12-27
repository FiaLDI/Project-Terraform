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

            // Создаём фасад статов
            Facade = new StatsFacade(isTurret: false);
            Debug.Log("[PlayerStats] StatsFacade created", this);

            // Получаем или создаём адаптер
            Adapter = GetComponent<StatsFacadeAdapter>();
            if (Adapter == null)
            {
                Debug.Log("[PlayerStats] StatsFacadeAdapter not found, adding component", this);
                Adapter = gameObject.AddComponent<StatsFacadeAdapter>();
            }

            // Инициализируем адаптер
            Adapter.Init(Facade);
            Debug.Log("[PlayerStats] StatsFacadeAdapter initialized", this);

            IsReady = true;
            Debug.Log("[PlayerStats] PlayerStats is READY ✅", this);

            // Уведомляем подписчиков
            OnStatsReady?.Invoke(this);
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
