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

        public static event System.Action<NetworkPlayer> OnLocalPlayerSpawned;

        private void Awake()
        {
            if (playerController == null)
                playerController = GetComponent<PlayerController>();
        }

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

            var registry = PlayerRegistry.Instance;
            if (registry == null)
            {
                Debug.LogError("[NetworkPlayer] PlayerRegistry not found (Instance == null)!", this);
                return;
            }

            registry.RegisterPlayer(gameObject);
            Debug.Log($"[NetworkPlayer] Player registered: {gameObject.name}", this);

            // Если не локальный владелец — дальше ничего не делаем
            if (!IsOwner || !Owner.IsLocalClient)
            {
                Debug.Log($"[NetworkPlayer] This is REMOTE player: {gameObject.name}", this);
                return;
            }

            // Инициализируем статы только для локального игрока
            InitializePlayerStats();

            // Устанавливаем локального игрока в реестре
            registry.SetLocalPlayer(gameObject);

            // Уведомляем слушателей (LocalPlayerController, и т.п.)
            OnLocalPlayerSpawned?.Invoke(this);

            Debug.Log($"[NetworkPlayer] Local player fully initialized: {gameObject.name} ✅", this);
        }

        private void InitializePlayerStats()
        {
            Debug.Log("[NetworkPlayer] Initializing player stats...", this);

            var playerStats = GetComponent<PlayerStats>();
            if (playerStats == null)
            {
                Debug.LogError("[NetworkPlayer] PlayerStats component not found!", gameObject);
                return;
            }

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
                Debug.Log($"[NetworkPlayer] Local player despawned: {gameObject.name}", this);

            var registry = PlayerRegistry.Instance;
            if (registry != null)
                registry.UnregisterPlayer(gameObject);
        }
    }
}
