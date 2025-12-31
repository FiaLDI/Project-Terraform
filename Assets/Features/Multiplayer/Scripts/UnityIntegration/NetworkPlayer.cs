using FishNet.Object;
using UnityEngine;
using Features.Abilities.Application;
using Features.Interaction.UnityIntegration;

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

        // =====================================================
        // SERVER
        // =====================================================

        public override void OnStartServer()
        {
            base.OnStartServer();

            if (playerController == null)
                playerController = GetComponent<PlayerController>();
        }

        // =====================================================
        // CLIENT
        // =====================================================

        public override void OnStartClient()
        {
            base.OnStartClient();

            Debug.Log(
                $"[NetworkPlayer] OnStartClient: {gameObject.name}, IsOwner={IsOwner}",
                this);

            gameObject.SetActive(true);

            var registry = PlayerRegistry.Instance;
            if (registry == null)
            {
                Debug.LogError("[NetworkPlayer] PlayerRegistry not found!", this);
                return;
            }

            // -------- register --------
            registry.RegisterPlayer(gameObject);

            // -------- remote --------
            if (!IsOwner)
            {
                Debug.Log($"[NetworkPlayer] REMOTE player: {gameObject.name}", this);

                if (playerController != null)
                    playerController.enabled = false;

                var nearby = GetComponent<NearbyInteractables>();
                if (nearby != null)
                    nearby.enabled = false;

                return;
            }

            // -------- local --------
            Debug.Log($"[NetworkPlayer] LOCAL player detected: {gameObject.name}", this);

            if (playerController != null)
                playerController.enabled = true;

            var nearbyLocal = GetComponent<NearbyInteractables>();
            if (nearbyLocal != null)
                nearbyLocal.Initialize(IsOwner);

            var abilities = GetComponent<AbilityCaster>();
            if (abilities != null)
                abilities.enabled = true;

            registry.SetLocalPlayer(gameObject);

            OnLocalPlayerSpawned?.Invoke(this);
        }

        // =====================================================
        // CLEANUP
        // =====================================================

        public override void OnStopClient()
        {
            base.OnStopClient();

            if (IsOwner && Owner.IsLocalClient)
                Debug.Log(
                    $"[NetworkPlayer] Local player despawned: {gameObject.name}",
                    this);

            var registry = PlayerRegistry.Instance;
            if (registry != null)
                registry.UnregisterPlayer(gameObject);
        }
    }
}
