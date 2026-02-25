using UnityEngine;
using FishNet.Object;
using Unity.Mathematics;
using System.Collections;
using Features.Biomes.Domain;
using Features.Biomes.Application;
using Features.Player.UnityIntegration;

namespace Features.Biomes.UnityIntegration
{
    public sealed class RuntimeWorldGenerator : NetworkBehaviour
    {
        [Header("World Settings")]
        public WorldConfig worldConfig;

        [Header("Systems Prefab (World-only)")]
        public GameObject systemsPrefab;

        [Header("Spawn Points")]
        [SerializeField] private ScenePlayerSpawnPoint spawnPointPrefab;
        [SerializeField, Min(1)] private int spawnPointCount = 4;
        [SerializeField] private float spawnRadius = 15f;

        [Header("Custom Prefab")]
        public GameObject customPrefab;

        [Header("Chunk Streaming")]
        public int loadDistance = 5;
        public int unloadDistance = 8;

        [Header("Spawn Settings")]
        public float spawnHeightCheck = 50f;

        private ChunkManager manager;
        private WorldProvider worldProvider;
        private Transform trackedPlayer;
        private int worldSeed;

        public static WorldConfig World { get; private set; }

        // ======================================================
        // SERVER
        // ======================================================

        public override void OnStartServer()
        {
            base.OnStartServer();
            StartCoroutine(ServerFlow());
        }

        private IEnumerator ServerFlow()
        {
            yield return WaitForWorldProvider();
            InitializeSeed(worldSeed);

            yield return ServerGenerateWorld();

            worldProvider.SetWorldReady();
        }

        // ======================================================
        // CLIENT
        // ======================================================

        public override void OnStartClient()
        {
            base.OnStartClient();

            if (IsServer && !IsClient)
                 return;

            StartCoroutine(ClientFlow());
        }

        private IEnumerator ClientFlow()
        {
            yield return WaitForWorldProvider();
            InitializeSeed(worldSeed);

            yield return ClientGenerateWorld();

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
        }

        // ======================================================
        // COMMON
        // ======================================================

        private IEnumerator WaitForWorldProvider()
        {
            while (true)
            {
                if (worldProvider == null)
                    worldProvider = FindObjectOfType<WorldProvider>();

                if (worldProvider != null && worldProvider.Seed.Value != 0)
                {
                    worldSeed = worldProvider.Seed.Value;
                    Debug.Log($"[world-gen] seed='{worldSeed}'");
                    yield break;
                }

                yield return null;
            }
        }

        private void InitializeSeed(int seed)
        {
            worldConfig.seed = seed;
            UnityEngine.Random.InitState(seed);
        }

        // ======================================================
        // CLIENT GENERATION
        // ======================================================

        private IEnumerator ClientGenerateWorld()
        {
            if (!BiomeRuntimeDatabase.Initialized)
                BiomeRuntimeDatabase.Build(worldConfig);

            manager = new ChunkManager(worldConfig);
            World = worldConfig;

            yield return null;

            Debug.Log("[WorldGen] Client world ready");
        }

        // ======================================================
        // SERVER GENERATION
        // ======================================================

        private IEnumerator ServerGenerateWorld()
        {
            if (!BiomeRuntimeDatabase.Initialized)
                BiomeRuntimeDatabase.Build(worldConfig);

            manager = new ChunkManager(worldConfig);
            World = worldConfig;

            manager.UpdateChunks(GetWorldCenterSpawn(), loadDistance, unloadDistance);
            manager.ProcessLoadQueue();

            yield return new WaitForFixedUpdate();

            if (systemsPrefab != null)
            {
                var systemsInstance = Instantiate(
                    systemsPrefab,
                    GetWorldCenterSpawn(),
                    Quaternion.identity
                );

                Spawn(systemsInstance);
            }

            if (spawnPointPrefab != null)
                SpawnPlayerSpawnPoints();

            yield return WaitForPhysicsReady();

            if (customPrefab != null)
                SpawnCustomPrefab();
        }

        // ======================================================
        // STREAMING (CLIENT ONLY)
        // ======================================================

        private void Update()
        {
            if (manager == null)
                return;

            if (IsServer && !IsClient)
                return;

            if (trackedPlayer == null)
                return;

            manager.UpdateChunks(trackedPlayer.position, loadDistance, unloadDistance);
            manager.ProcessLoadQueue();
        }

        // ======================================================
        // HELPERS
        // ======================================================

        private IEnumerator WaitForPhysicsReady()
        {
            int safety = 0;

            while (safety < 50)
            {
                Vector3 testPoint = GetWorldCenterSpawn();

                if (Physics.Raycast(
                        testPoint + Vector3.up * 10f,
                        Vector3.down,
                        50f))
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

            if (Physics.Raycast(origin, Vector3.down, out var hit, spawnHeightCheck * 2f))
                return hit.point + Vector3.up * 2f;

            float h = worldConfig.GetHeight(new float2(cx, cz));
            return new Vector3(cx, h + 2f, cz);
        }

        private void SpawnCustomPrefab()
        {
            var pos = GetWorldCenterSpawn();
            Instantiate(customPrefab, pos, Quaternion.identity);
        }

        private void SpawnPlayerSpawnPoints()
        {
            Vector3 center = GetWorldCenterSpawn();

            for (int i = 0; i < spawnPointCount; i++)
            {
                Vector2 offset2D = UnityEngine.Random.insideUnitCircle * spawnRadius;
                Vector3 origin = center + new Vector3(offset2D.x, spawnHeightCheck, offset2D.y);

                Vector3 pos = origin;

                if (Physics.Raycast(origin, Vector3.down, out var hit, spawnHeightCheck * 2f))
                    pos = hit.point + Vector3.up * 1.5f;

                var sp = Instantiate(spawnPointPrefab, pos, Quaternion.identity);
                sp.name = $"WorldSpawnPoint_{i}";
            }
        }
    }
}