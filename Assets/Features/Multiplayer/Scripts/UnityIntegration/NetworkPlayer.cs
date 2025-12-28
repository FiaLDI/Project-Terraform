using FishNet.Object;
using UnityEngine;
using Features.Stats.UnityIntegration;
using Features.Abilities.Application;



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
            
            InitializePlayerStats();
        }


        public override void OnStartClient()
        {
            base.OnStartClient();


            Debug.Log($"[NetworkPlayer] OnStartClient: {gameObject.name}, IsOwner={IsOwner}", this);


            gameObject.SetActive(true);


            var registry = PlayerRegistry.Instance;
            if (registry == null)
            {
                Debug.LogError("[NetworkPlayer] PlayerRegistry not found!", this);
                return;
            }


            registry.RegisterPlayer(gameObject);
            InitializePlayerStats();


            // 🟢 ГЛАВНОЕ: НЕ отключаем AbilityCaster здесь!
            // Отключение будет в PlayerStateNetAdapter.RpcApplyClassWithAbilities()
            // когда абилити уже загружены с сервера
            
            
            if (!IsOwner)
            {
                Debug.Log($"[NetworkPlayer] REMOTE player: {gameObject.name}", this);
                
                var controller = GetComponent<PlayerController>();
                if (controller != null)
                    controller.enabled = false;
                
                // ❌ НЕ отключаем AbilityCaster здесь - это сделает PlayerStateNetAdapter
                return;
            }


            // 🟢 Для ЛОКАЛЬНОГО игрока
            Debug.Log($"[NetworkPlayer] LOCAL player detected: {gameObject.name}", this);


            if (GetComponent<PlayerController>() != null)
                GetComponent<PlayerController>().enabled = true;


            var localAbilities = GetComponent<AbilityCaster>();
            if (localAbilities != null)
                localAbilities.enabled = true;


            registry.SetLocalPlayer(gameObject);
            
            OnLocalPlayerSpawned?.Invoke(this);
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
