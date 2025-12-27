using FishNet.Object;
using UnityEngine;
using Features.Stats.UnityIntegration;


namespace Features.Player.UnityIntegration
{
    [RequireComponent(typeof(PlayerController))]
    public sealed class NetworkPlayer : NetworkBehaviour
    {
        [SerializeField] private PlayerController playerController;
        public PlayerController Controller => playerController;

        /// <summary>
        /// Вызывается ТОЛЬКО для локального игрока
        /// </summary>
        public static event System.Action<NetworkPlayer> OnLocalPlayerSpawned;

        public override void OnStartServer()
        {
            base.OnStartServer();

            if (playerController == null)
                playerController = GetComponent<PlayerController>();
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            Debug.Log($"[NetworkPlayer] OnStartClient: {gameObject.name}, IsOwner={IsOwner}", this);

            // Регистрируем игрока в реестре
            if (PlayerRegistry.Instance == null)
            {
                Debug.LogError("[NetworkPlayer] PlayerRegistry not found!", this);
                return;
            }

            PlayerRegistry.Instance.RegisterPlayer(gameObject);
            Debug.Log($"[NetworkPlayer] Player registered: {gameObject.name}", this);

            // Проверяем что это локальный игрок
            if (!IsOwner || !Owner.IsLocalClient)
            {
                Debug.Log($"[NetworkPlayer] This is REMOTE player: {gameObject.name}", this);
                return;
            }

            // ✅ Инициализируем статы ТОЛЬКО для локального игрока
            InitializePlayerStats();

            // Устанавливаем как локального игрока в реестре
            PlayerRegistry.Instance.SetLocalPlayer(gameObject);

            // Уведомляем что локальный игрок готов
            OnLocalPlayerSpawned?.Invoke(this);

            Debug.Log($"[NetworkPlayer] Local player fully initialized: {gameObject.name} ✅", this);
        }

        /// <summary>
        /// Инициализирует статы игрока.
        /// </summary>
        private void InitializePlayerStats()
        {
            Debug.Log("[NetworkPlayer] Initializing player stats...", this);

            // Получаем компонент PlayerStats
            var playerStats = GetComponent<PlayerStats>();
            if (playerStats == null)
            {
                Debug.LogError("[NetworkPlayer] PlayerStats component not found!", gameObject);
                return;
            }

            // Инициализируем статы
            playerStats.Init();

            if (!playerStats.IsReady)
            {
                Debug.LogError("[NetworkPlayer] PlayerStats initialization failed!", gameObject);
                return;
            }

            Debug.Log("[NetworkPlayer] PlayerStats initialized successfully ✅", this);
        }

        public override void OnStopClient()
        {
            base.OnStopClient();

            if (IsOwner && Owner.IsLocalClient)
            {
                Debug.Log($"[NetworkPlayer] Local player despawned: {gameObject.name}", this);
            }

            if (PlayerRegistry.Instance != null)
                PlayerRegistry.Instance.UnregisterPlayer(gameObject);
        }
    }
}
