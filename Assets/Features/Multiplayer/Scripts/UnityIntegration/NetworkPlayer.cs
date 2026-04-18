using System.Collections;
using FishNet;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Features.Abilities.Application;
using Features.Interaction.UnityIntegration;
using Features.Stats.Domain;
using Features.Stats.UnityIntegration;
using UnityEngine;

namespace Features.Player.UnityIntegration
{
    [RequireComponent(typeof(PlayerController))]
    public sealed class NetworkPlayer : NetworkBehaviour
    {
        [Header("Refs")]
        [SerializeField] private PlayerController playerController;
        [SerializeField] private PlayerVisualController visual;
        [SerializeField] private PlayerNetworkController networkController;
        [SerializeField] private DeterministicMovement movement;
        [SerializeField] private UnifiedStatsUpdateSystem unifiedStatsUpdateSystem;
        [SerializeField] private PlayerCameraController playerCameraController;

        [Header("Death")]
        [SerializeField] private float respawnDelay = 4f;

        private readonly SyncVar<bool> isDead = new();

        private IStatsOwner statsOwner;
        private IHealthStats health;
        private Coroutine bindHealthRoutine;
        private Coroutine respawnRoutine;
        private bool hasSeenAliveHealth;

        public PlayerController Controller => playerController;
        public bool IsDead => isDead.Value;
        public float RespawnDelay => respawnDelay;

        public static event System.Action<NetworkPlayer> OnLocalPlayerSpawned;

        private void Awake()
        {
            if (playerController == null)
                playerController = GetComponent<PlayerController>();

            if (visual == null)
                visual = GetComponent<PlayerVisualController>();

            if (networkController == null)
                networkController = GetComponent<PlayerNetworkController>();

            if (movement == null)
                movement = GetComponent<DeterministicMovement>();

            if (unifiedStatsUpdateSystem == null)
                unifiedStatsUpdateSystem = GetComponent<UnifiedStatsUpdateSystem>();

            if (playerCameraController == null)
                playerCameraController = GetComponent<PlayerCameraController>();
        }

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();
            isDead.OnChange += OnDeadChanged;
        }

        public override void OnStopNetwork()
        {
            isDead.OnChange -= OnDeadChanged;
            base.OnStopNetwork();
        }

        public override void OnStartServer()
        {
            base.OnStartServer();

            var registry = PlayerRegistry.Instance;
            if (registry != null)
                registry.RegisterPlayer(gameObject);

            if (bindHealthRoutine != null)
                StopCoroutine(bindHealthRoutine);

            bindHealthRoutine = StartCoroutine(BindHealthRoutine());
        }

        public override void OnStopServer()
        {
            if (bindHealthRoutine != null)
            {
                StopCoroutine(bindHealthRoutine);
                bindHealthRoutine = null;
            }

            if (respawnRoutine != null)
            {
                StopCoroutine(respawnRoutine);
                respawnRoutine = null;
            }

            UnbindHealth();

            var registry = PlayerRegistry.Instance;
            if (registry != null)
                registry.UnregisterPlayer(gameObject);

            base.OnStopServer();
        }

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
            }
            else
            {
                Debug.Log("[fix-net] IsOwner TRUE in OnStartClient", this);
                SetupAsLocal();
            }

            ApplyDeadState(isDead.Value);
        }

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

            ApplyDeadState(isDead.Value);
        }

        private void SetupAsLocal()
        {
            var registry = PlayerRegistry.Instance;
            if (registry == null)
                return;

            if (!isDead.Value)
                EnableLocalComponents();

            registry.SetLocalPlayer(gameObject);
            OnLocalPlayerSpawned?.Invoke(this);

            Debug.Log($"[NetworkPlayer] Set as LOCAL player: {name}", this);
        }

        public override void OnStopClient()
        {
            base.OnStopClient();

            var registry = PlayerRegistry.Instance;
            if (registry != null)
                registry.UnregisterPlayer(gameObject);

            Debug.Log($"[NetworkPlayer] Stopped client for {name}", this);
        }

        public void SetLocalGameplayEnabled(bool enabled)
        {
            if (!IsOwner)
                return;

            if (playerCameraController != null)
                playerCameraController.SetLookEnabled(enabled);

            if (enabled)
                EnableLocalComponents();
            else
                DisableLocalComponents();
        }

        private IEnumerator BindHealthRoutine()
        {
            while (IsServerStarted)
            {
                if (TryBindHealth())
                {
                    bindHealthRoutine = null;
                    yield break;
                }

                yield return null;
            }

            bindHealthRoutine = null;
        }

        private bool TryBindHealth()
        {
            if (health != null)
                return true;

            statsOwner = GetComponent<IStatsOwner>();
            if (statsOwner == null || !statsOwner.IsReady)
                return false;

            health = statsOwner.Facade?.Health;
            if (health == null)
                return false;

            health.OnHealthChanged += HandleHealthChanged;
            hasSeenAliveHealth = health.MaxHp > 0f && health.CurrentHp > 0f;
            return true;
        }

        private void UnbindHealth()
        {
            if (health != null)
                health.OnHealthChanged -= HandleHealthChanged;

            health = null;
            statsOwner = null;
        }

        private void HandleHealthChanged(float current, float max)
        {
            if (!IsServerInitialized || isDead.Value)
                return;

            if (max > 0f && current > 0f)
            {
                hasSeenAliveHealth = true;
                return;
            }

            if (!hasSeenAliveHealth || max <= 0f)
                return;

            if (current <= 0f)
                DieServer();
        }

        [Server]
        private void DieServer()
        {
            if (isDead.Value)
                return;

            isDead.Value = true;
            ApplyDeadState(true);

            if (respawnRoutine != null)
                StopCoroutine(respawnRoutine);

            respawnRoutine = StartCoroutine(RespawnAfterDelay());
        }

        private IEnumerator RespawnAfterDelay()
        {
            float remaining = Mathf.Max(0f, respawnDelay);

            while (remaining > 0f)
            {
                if (!IsServerStarted)
                    yield break;

                remaining -= Time.deltaTime;
                yield return null;
            }

            respawnRoutine = null;
            RespawnServer();
        }

        [Server]
        private void RespawnServer()
        {
            var root = ServerCompositionRoot.I;
            if (root == null || root.Sessions == null || root.Spawner == null)
                return;

            if (Owner == null)
                return;

            var session = root.Sessions.GetSessionByClient(Owner.ClientId);
            if (session == null || !session.IsOnline)
                return;

            root.Spawner.RespawnSession(session);
        }

        private void OnDeadChanged(bool prev, bool next, bool asServer)
        {
            ApplyDeadState(next);
        }

        private void ApplyDeadState(bool dead)
        {
            if (movement != null)
                movement.IsFrozen = dead;

            if (networkController != null)
                networkController.enabled = !dead;

            if (unifiedStatsUpdateSystem != null && IsServerStarted)
                unifiedStatsUpdateSystem.enabled = !dead;

            if (IsClientInitialized && IsOwner)
                SetLocalGameplayEnabled(!dead);
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
