using FishNet.Object;
using UnityEngine;

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

            if (PlayerRegistry.Instance == null)
            {
                Debug.LogError("[NetworkPlayer] PlayerRegistry missing");
                return;
            }
            PlayerRegistry.Instance?.RegisterPlayer(gameObject);

            // локальный игрок
            if (!IsOwner || !Owner.IsLocalClient)
                return;

            Debug.Log($"[NetworkPlayer] Local player spawned: {name}");

            PlayerRegistry.Instance?.SetLocalPlayer(gameObject);
            OnLocalPlayerSpawned?.Invoke(this);
        }

        public override void OnStopClient()
        {
            base.OnStopClient();

            if (IsOwner && Owner.IsLocalClient)
            {
                Debug.Log($"[NetworkPlayer] Local player despawned: {name}");
            }

            PlayerRegistry.Instance?.UnregisterPlayer(gameObject);
        }
    }
}
