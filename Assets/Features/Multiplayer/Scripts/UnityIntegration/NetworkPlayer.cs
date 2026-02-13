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
                $"[NetworkPlayer] Spawned: {name}, IsOwner={IsOwner}",
                this);

            var registry = PlayerRegistry.Instance;
            if (registry == null)
            {
                Debug.LogError("[NetworkPlayer] PlayerRegistry missing!", this);
                return;
            }

            registry.RegisterPlayer(gameObject);

            if (!IsOwner)
            {
                DisableLocalComponents();
                return;
            }

            EnableLocalComponents();
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

        private void DisableLocalComponents()
        {
            if (playerController != null)
                playerController.enabled = false;

            var nearby = GetComponent<NearbyInteractables>();
            if (nearby != null)
                nearby.enabled = false;

            var abilities = GetComponent<AbilityCaster>();
            if (abilities != null)
                abilities.enabled = false;
        }

        private void EnableLocalComponents()
        {
            if (playerController != null)
                playerController.enabled = true;

            var nearby = GetComponent<NearbyInteractables>();
            if (nearby != null)
                nearby.Initialize(true);

            var abilities = GetComponent<AbilityCaster>();
            if (abilities != null)
                abilities.enabled = true;
        }
    }
}
