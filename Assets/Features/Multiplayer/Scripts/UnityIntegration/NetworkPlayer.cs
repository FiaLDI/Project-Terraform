using FishNet.Object;
using FishNet.Connection;
using UnityEngine;
using Features.Abilities.Application;
using Features.Interaction.UnityIntegration;
using FishNet;

namespace Features.Player.UnityIntegration
{
    [RequireComponent(typeof(PlayerController))]
    public sealed class NetworkPlayer : NetworkBehaviour
    {
        [SerializeField] private PlayerController playerController;
        [SerializeField] private PlayerVisualController visual;
        public PlayerController Controller => playerController;

        public static event System.Action<NetworkPlayer> OnLocalPlayerSpawned;

        private void Awake()
        {
            if (playerController == null)
                playerController = GetComponent<PlayerController>();
            
            visual = GetComponent<PlayerVisualController>();
        }

        // =====================================================
        // SERVER
        // =====================================================

        public override void OnStartServer()
        {
            base.OnStartServer();

            if (playerController == null)
                playerController = GetComponent<PlayerController>();
            
            var registry = PlayerRegistry.Instance;
            if (registry != null)
                registry.RegisterPlayer(gameObject);
        }

        public override void OnStopServer()
        {
            base.OnStopServer();

            var registry = PlayerRegistry.Instance;
            if (registry != null)
                registry.UnregisterPlayer(gameObject);
        }

        // =====================================================
        // CLIENT SPAWN
        // =====================================================

        public override void OnStartClient()
        {
            base.OnStartClient();

            visual?.SetLocal(IsOwner);

           Debug.Log(
                $"[fix-net] OnStartClient -> name={name}, " +
                $"IsOwner={IsOwner}, " +
                $"OwnerId={Owner?.ClientId}, ",
                this
            );

            var registry = PlayerRegistry.Instance;
            if (registry == null)
            {
                Debug.LogError("[fix-net] PlayerRegistry missing!", this);
                return;
            }

            registry.RegisterPlayer(gameObject);

            if (!IsOwner)
            {
                Debug.Log("[fix-net] Not owner, disabling local components", this);
                DisableLocalComponents();
                return;
            }

            Debug.Log("[fix-net] IsOwner TRUE in OnStartClient", this);
            SetupAsLocal();
        }

        // =====================================================
        // OWNERSHIP CHANGED (КЛЮЧЕВОЕ)
        // =====================================================

        public override void OnOwnershipClient(NetworkConnection prevOwner)
        {
            base.OnOwnershipClient(prevOwner);

            visual?.SetLocal(IsOwner);

            int localId = InstanceFinder.ClientManager.Connection.ClientId;

            Debug.Log(
                $"[fix-net] OnOwnershipClient -> name={name}, " +
                $"IsOwner={IsOwner}, " +
                $"OwnerId={Owner?.ClientId}, " +
                $"LocalId={localId}",
                this
            );

            if (IsOwner)
            {
                Debug.Log("[fix-net] Ownership became LOCAL", this);
                SetupAsLocal();
            }
            else
            {
                Debug.Log("[fix-net] Ownership removed", this);
                DisableLocalComponents();
            }
        }

        private void SetupAsLocal()
        {
            var registry = PlayerRegistry.Instance;
            if (registry == null)
                return;

            EnableLocalComponents();
            registry.SetLocalPlayer(gameObject);

            OnLocalPlayerSpawned?.Invoke(this);

            Debug.Log($"[NetworkPlayer] Set as LOCAL player: {name}", this);
        }

        // =====================================================
        // CLEANUP
        // =====================================================

        public override void OnStopClient()
        {
            base.OnStopClient();

            var registry = PlayerRegistry.Instance;
            if (registry != null)
                registry.UnregisterPlayer(gameObject);

            Debug.Log($"[NetworkPlayer] Stopped client for {name}", this);
        }

        // =====================================================
        // LOCAL COMPONENT CONTROL
        // =====================================================

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

            // Remote player replicas should not participate in client-side
            // CharacterController collisions, otherwise the local owner can
            // predict against different physics than the server.
            var controller = GetComponent<CharacterController>();
            if (controller != null && !IsServer)
                controller.enabled = false;
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

            var controller = GetComponent<CharacterController>();
            if (controller != null)
                controller.enabled = true;
        }
    }
}
