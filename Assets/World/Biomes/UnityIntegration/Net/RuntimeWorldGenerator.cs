using UnityEngine;
using FishNet.Object;
using Unity.Mathematics;
using System.Collections;
using System.Collections.Generic;
using Features.Player.UnityIntegration;
using Biomes.Application;
using Biomes.Data;

namespace Biomes.UnityIntegration
{
    public sealed class RuntimeWorldGenerator : NetworkBehaviour
    {
        [Header("World Settings")]
        public WorldConfig worldConfig;
        [SerializeField] private WorldConfig[] availableWorldConfigs;

        [Header("Spawn Points")]
        [SerializeField] private ScenePlayerSpawnPoint spawnPointPrefab;
        [SerializeField, Min(1)] private int spawnPointCount = 4;
        [SerializeField] private float spawnRadius = 15f;

        [Header("Custom Prefab")]
        public GameObject customPrefab;

        [Header("World Exit")]
        [SerializeField] private bool spawnExitBeacon = true;
        [SerializeField] private GameObject exitBeaconPrefab;
        [SerializeField] private float exitBeaconMinDistance = 90f;
        [SerializeField] private float exitBeaconMaxDistance = 160f;
        [SerializeField] private float exitBeaconTriggerRadius = 5f;
        [SerializeField] private int exitBeaconPlacementAttempts = 64;
        [SerializeField] private bool spawnCheckpointOnBeaconRoute = true;
        [SerializeField, Range(0.1f, 0.9f)] private float checkpointRouteT = 0.45f;
        [SerializeField] private float checkpointTriggerRadius = 4f;

        [Header("Chunk Streaming")]
        public int loadDistance = 5;
        public int unloadDistance = 8;

        [Header("Spawn Settings")]
        public float spawnHeightCheck = 20f;

        private ChunkManager serverManager;
        private ChunkManager clientManager;
        private WorldProvider worldProvider;
        private Transform trackedPlayer;
        private int worldSeed;
        private string selectedWorldConfigId;
        private WorldRunConfig runConfig;
        private int groundMask;
        private bool customPrefabSpawned;
        private GameObject exitBeaconObject;
        private GameObject checkpointObject;
        private readonly List<Vector3> serverStreamingTargets = new();

        public static WorldConfig World { get; private set; }

        public override void OnStartServer()
        {
            base.OnStartServer();
            ResetRuntimeState(clearManagers: false);
            StartCoroutine(ServerFlow());
        }

        private IEnumerator ServerFlow()
        {
            yield return WaitForWorldProvider();
            ApplySelectedWorldConfig();
            groundMask = LayerMask.GetMask("Ground");
            InitializeSeed(worldSeed);

            yield return ServerGenerateWorld();

            worldProvider.SetWorldReady();
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            if (IsServer)
            {
                StartCoroutine(HostClientFlow());
                return;
            }

            ResetRuntimeState(clearManagers: false);
            StartCoroutine(ClientFlow());
        }

        private IEnumerator HostClientFlow()
        {
            yield return WaitForWorldProvider();
            ApplySelectedWorldConfig();
            InitializeSeed(worldSeed);

            World = worldConfig;
            SpawnCustomPrefabIfNeeded();
            SpawnExitBeaconIfNeeded(includeServerTriggers: false);
        }

        private IEnumerator ClientFlow()
        {
            yield return WaitForWorldProvider();
            ApplySelectedWorldConfig();
            InitializeSeed(worldSeed);

            yield return ClientGenerateWorld();

            PlayerRegistry.SubscribeLocalPlayerReady(OnLocalPlayerReady);
            yield return WaitForLocalPlayerController();
        }

        private IEnumerator WaitForLocalPlayerController()
        {
            while (LocalPlayerController.I == null ||
                LocalPlayerController.I.BoundPlayer == null)
            {
                yield return null;
            }

            trackedPlayer = LocalPlayerController.I.BoundPlayer.transform;

            Debug.Log("[WorldGen] Streaming bound to LocalPlayerController");
        }

        private void OnLocalPlayerReady(PlayerRegistry registry)
        {
            trackedPlayer = registry.LocalPlayer.transform;
            Debug.Log("[WorldGen] Local player attached to streaming");
        }

        private void OnDestroy()
        {
            PlayerRegistry.UnsubscribeLocalPlayerReady(OnLocalPlayerReady);
            ResetRuntimeState(clearManagers: true);
        }

        private IEnumerator WaitForWorldProvider()
        {
            while (true)
            {
                if (worldProvider == null)
                    worldProvider = FindObjectOfType<WorldProvider>();

                if (worldProvider != null && worldProvider.HasBootstrap.Value)
                {
                    worldSeed = worldProvider.Seed.Value;
                    selectedWorldConfigId = worldProvider.WorldConfigId.Value;
                    runConfig = worldProvider.GetRunConfig();
                    WorldRunContext.Set(runConfig);
                    Debug.Log(
                        $"[world-gen] seed='{worldSeed}' level='{runConfig.worldLevel}' difficulty='{runConfig.difficulty}'");
                    yield break;
                }

                yield return null;
            }
        }

        private void InitializeSeed(int seed)
        {
            if (worldConfig == null)
                return;

            worldConfig.seed = seed;
            UnityEngine.Random.InitState(seed);
        }

        private IEnumerator ClientGenerateWorld()
        {
            if (!BiomeRuntimeDatabase.IsBuiltFor(worldConfig))
                BiomeRuntimeDatabase.Build(worldConfig);

            clientManager = new ChunkManager(worldConfig);
            World = worldConfig;

            yield return null;

            SpawnCustomPrefabIfNeeded();
            SpawnExitBeaconIfNeeded(includeServerTriggers: false);

            Debug.Log("[WorldGen] Client world ready");
        }

        private IEnumerator ServerGenerateWorld()
        {
            if (!BiomeRuntimeDatabase.IsBuiltFor(worldConfig))
                BiomeRuntimeDatabase.Build(worldConfig);

            serverManager = new ChunkManager(worldConfig);
            World = worldConfig;

            serverManager.UpdateChunks(GetWorldCenterSpawn(), loadDistance, unloadDistance);
            serverManager.ProcessLoadQueue();

            yield return new WaitForFixedUpdate();
            yield return WaitForPhysicsReady();

            if (spawnPointPrefab != null)
                SpawnPlayerSpawnPoints();

            SpawnExitBeaconIfNeeded(includeServerTriggers: true);
        }

        private void Update()
        {
            UpdateServerStreaming();
            UpdateClientStreaming();
        }

        private void UpdateServerStreaming()
        {
            if (!IsServerStarted || serverManager == null)
                return;

            var registry = PlayerRegistry.Instance;
            if (registry == null)
                return;

            serverStreamingTargets.Clear();

            var players = registry.Players;
            for (int i = 0; i < players.Count; i++)
            {
                var player = players[i];
                if (player == null)
                    continue;

                serverStreamingTargets.Add(player.transform.position);
            }

            if (serverStreamingTargets.Count == 0)
                return;

            serverManager.UpdateChunks(serverStreamingTargets, loadDistance, unloadDistance);
            serverManager.ProcessLoadQueue();
        }

        private void UpdateClientStreaming()
        {
            if (!IsClientStarted || clientManager == null)
                return;

            if (trackedPlayer == null)
                return;

            clientManager.UpdateChunks(trackedPlayer.position, loadDistance, unloadDistance);
            clientManager.ProcessLoadQueue();
        }

        private IEnumerator WaitForPhysicsReady()
        {
            int safety = 0;

            while (safety < 50)
            {
                Vector3 testPoint = GetWorldCenterSpawn();

                if (Physics.Raycast(
                        testPoint + Vector3.up * 10f,
                        Vector3.down,
                        50f, groundMask))
                {
                    yield break;
                }

                safety++;
                yield return new WaitForFixedUpdate();
            }

            Debug.LogWarning("[WorldGen] Physics readiness timeout");
        }

        private Vector3 GetWorldCenterSpawn()
        {
            int cs = worldConfig.chunkSize;
            float cx = cs * 0.5f;
            float cz = cs * 0.5f;

            Vector3 origin = new Vector3(cx, spawnHeightCheck, cz);

            if (Physics.Raycast(origin, Vector3.down, out var hit, spawnHeightCheck * 2f, groundMask))
                return hit.point + Vector3.up * 2f;

            float h = worldConfig.GetHeight(new float2(cx, cz));
            return new Vector3(cx, h + 2f, cz);
        }

        private void SpawnPlayerSpawnPoints()
        {
            Vector3 center = GetWorldCenterSpawn();

            for (int i = 0; i < spawnPointCount; i++)
            {
                Vector2 offset2D = UnityEngine.Random.insideUnitCircle * spawnRadius;
                Vector3 origin = center + new Vector3(offset2D.x, spawnHeightCheck, offset2D.y);

                Vector3 pos = origin;

                if (Physics.Raycast(origin, Vector3.down, out var hit, spawnHeightCheck * 2f, groundMask))
                    pos = hit.point + Vector3.up * 1.5f;

                var sp = Instantiate(spawnPointPrefab, pos, Quaternion.identity);
                sp.name = $"WorldSpawnPoint_{i}";
            }
        }

        private void SpawnCustomPrefabIfNeeded()
        {
            if (customPrefabSpawned || customPrefab == null)
                return;

            var pos = GetWorldCenterSpawn();
            Instantiate(customPrefab, pos, Quaternion.identity);
            customPrefabSpawned = true;
        }

        private void SpawnExitBeaconIfNeeded(bool includeServerTriggers)
        {
            if (!spawnExitBeacon || worldConfig == null)
                return;

            if (exitBeaconObject != null)
            {
                if (includeServerTriggers)
                {
                    AttachBeaconTrigger(exitBeaconObject);

                    if (checkpointObject != null)
                        AttachCheckpointTrigger(checkpointObject);
                }

                return;
            }

            Vector3 center = GetWorldCenterSpawn();
            if (!WorldPlacementService.TryFindReachablePoint(
                    worldConfig,
                    center,
                    exitBeaconMinDistance,
                    exitBeaconMaxDistance,
                    exitBeaconPlacementAttempts,
                    0x4B1D,
                    out var beaconPosition))
            {
                Debug.LogWarning("[WorldGen] Failed to place exit beacon");
                return;
            }

            exitBeaconObject = CreateBeaconObject(beaconPosition, includeServerTriggers);
            Debug.Log($"[WorldGen] Exit beacon placed at {beaconPosition}", exitBeaconObject);

            if (spawnCheckpointOnBeaconRoute)
                checkpointObject = CreateCheckpointOnRoute(center, beaconPosition, includeServerTriggers);
        }

        private GameObject CreateBeaconObject(Vector3 position, bool includeServerTriggers)
        {
            var root = exitBeaconPrefab != null
                ? Instantiate(exitBeaconPrefab, position, Quaternion.identity)
                : new GameObject("WorldExitBeacon");

            root.name = "WorldExitBeacon";
            root.transform.position = position;
            root.transform.SetParent(transform, true);

            if (includeServerTriggers)
                AttachBeaconTrigger(root);

            if (exitBeaconPrefab != null)
                return root;

            var pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pillar.name = "BeaconPillar";
            pillar.transform.SetParent(root.transform, false);
            pillar.transform.localPosition = Vector3.up * 2.5f;
            pillar.transform.localScale = new Vector3(1.4f, 2.5f, 1.4f);
            RemoveCollider(pillar);

            var signal = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            signal.name = "BeaconSignal";
            signal.transform.SetParent(root.transform, false);
            signal.transform.localPosition = Vector3.up * 5.4f;
            signal.transform.localScale = Vector3.one * 2.2f;
            RemoveCollider(signal);

            return root;
        }

        private GameObject CreateCheckpointOnRoute(Vector3 center, Vector3 beaconPosition, bool includeServerTriggers)
        {
            Vector3 routePoint = Vector3.Lerp(center, beaconPosition, checkpointRouteT);
            Vector3 position = WorldPlacementService.SnapToGround(worldConfig, routePoint, 0.08f);

            var root = new GameObject("WorldCheckpoint");
            root.transform.position = position;
            root.transform.SetParent(transform, true);

            if (includeServerTriggers)
                AttachCheckpointTrigger(root);

            var disk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            disk.name = "CheckpointMarker";
            disk.transform.SetParent(root.transform, false);
            disk.transform.localScale = new Vector3(3f, 0.08f, 3f);
            RemoveCollider(disk);

            return root;
        }

        private static void RemoveCollider(GameObject go)
        {
            var collider = go.GetComponent<Collider>();
            if (collider != null)
                UnityEngine.Object.Destroy(collider);
        }

        private void AttachBeaconTrigger(GameObject root)
        {
            EnsureKinematicTriggerBody(root);

            if (root.GetComponent<WorldExitBeacon>() == null)
                root.AddComponent<WorldExitBeacon>();

            var trigger = root.GetComponent<BoxCollider>();
            if (trigger == null)
                trigger = root.AddComponent<BoxCollider>();

            trigger.isTrigger = true;
        }

        private void AttachCheckpointTrigger(GameObject root)
        {
            EnsureKinematicTriggerBody(root);

            if (root.GetComponent<WorldCheckpointTrigger>() == null)
                root.AddComponent<WorldCheckpointTrigger>();

            var trigger = root.GetComponent<SphereCollider>();
            if (trigger == null)
                trigger = root.AddComponent<SphereCollider>();

            trigger.isTrigger = true;
            trigger.radius = Mathf.Max(0.5f, checkpointTriggerRadius);
        }

        private static void EnsureKinematicTriggerBody(GameObject root)
        {
            var body = root.GetComponent<Rigidbody>();
            if (body == null)
                body = root.AddComponent<Rigidbody>();

            body.useGravity = false;
            body.isKinematic = true;
            body.constraints = RigidbodyConstraints.FreezeAll;
        }

        private void ApplySelectedWorldConfig()
        {
            var resolved = ResolveWorldConfig(selectedWorldConfigId);
            if (resolved == null)
            {
                Debug.LogError("[WorldGen] Failed to resolve WorldConfig for runtime generation");
                return;
            }

            worldConfig = resolved;
        }

        private WorldConfig ResolveWorldConfig(string configId)
        {
            if (worldConfig != null && (string.IsNullOrEmpty(configId) || worldConfig.name == configId))
                return worldConfig;

            if (!string.IsNullOrEmpty(configId) && availableWorldConfigs != null)
            {
                for (int i = 0; i < availableWorldConfigs.Length; i++)
                {
                    var candidate = availableWorldConfigs[i];
                    if (candidate != null && candidate.name == configId)
                        return candidate;
                }
            }

            if (!string.IsNullOrEmpty(configId) && worldConfig != null)
            {
                Debug.LogWarning($"[WorldGen] WorldConfig '{configId}' not found, falling back to '{worldConfig.name}'");
                return worldConfig;
            }

            if (worldConfig != null)
                return worldConfig;

            if (availableWorldConfigs != null)
            {
                for (int i = 0; i < availableWorldConfigs.Length; i++)
                {
                    if (availableWorldConfigs[i] != null)
                        return availableWorldConfigs[i];
                }
            }

            return null;
        }

        private void ResetRuntimeState(bool clearManagers)
        {
            if (clearManagers)
            {
                serverManager?.ClearAll();
                clientManager?.ClearAll();
            }

            serverManager = null;
            clientManager = null;
            trackedPlayer = null;
            worldProvider = null;
            runConfig = null;
            customPrefabSpawned = false;

            if (exitBeaconObject != null)
                Destroy(exitBeaconObject);

            if (checkpointObject != null)
                Destroy(checkpointObject);

            exitBeaconObject = null;
            checkpointObject = null;
            serverStreamingTargets.Clear();

            World = null;
            WorldRunContext.Clear();

            ChunkedGameObjectStorage.ClearAll();
            BiomeRuntimeDatabase.Dispose();
            EnemyBiomeCounter.ClearAll();
            EnemyWorldManager.Instance?.ClearRuntimeState();
            ChunkedInstanceLODSystem.Instance?.ClearRuntimeState();
            InstancedSpawnerSystem.Instance?.ClearRuntimeState();
        }
    }
}
