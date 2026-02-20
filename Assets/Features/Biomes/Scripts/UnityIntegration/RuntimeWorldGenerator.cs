using UnityEngine;
using FishNet.Object;
using Unity.Mathematics;
using System.Collections;
using Features.Biomes.Domain;
using Features.Biomes.Application;

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
        private GameObject systemsInstance;

        public static WorldConfig World { get; private set; }

        public static event System.Action<int> OnWorldReady;

        // ======================================================
        // SERVER
        // ======================================================

        private void Start()
{
    Debug.Log($"Generator Start | IsSpawned={IsSpawned} | IsServer={IsServer} | IsClient={IsClient}");
}

        public override void OnStartServer()
        {
            base.OnStartServer();

            if (worldConfig == null)
            {
                Debug.LogError("[RuntimeWorldGenerator] WorldConfig is NULL!");
                return;
            }

            StartCoroutine(ServerGenerateWorld());
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            if (IsServer)
                return;

            StartCoroutine(ClientGenerateWorld());
        }

        private IEnumerator ClientGenerateWorld()
        {
            if (!BiomeRuntimeDatabase.Initialized)
                BiomeRuntimeDatabase.Build(worldConfig);

            manager = new ChunkManager(worldConfig);
            World = worldConfig;

            manager.UpdateChunks(Vector3.zero, loadDistance, unloadDistance);
            manager.ProcessLoadQueue();

            yield return null;

            Debug.Log("[WorldGen] Client world ready");
        }

        private IEnumerator ServerGenerateWorld()
        {
            Debug.Log("[WorldGen] Starting generation...");

            // 1️Биомы
            if (!BiomeRuntimeDatabase.Initialized)
                BiomeRuntimeDatabase.Build(worldConfig);

            // ChunkManager
            manager = new ChunkManager(worldConfig);
            World = worldConfig;

            manager.UpdateChunks(Vector3.zero, loadDistance, unloadDistance);
            manager.ProcessLoadQueue();

            yield return new WaitForFixedUpdate();

            // Мир-системы
            if (systemsPrefab != null)
            {
                systemsInstance = Instantiate(
                    systemsPrefab,
                    GetWorldCenterSpawn(),
                    Quaternion.identity
                );

                Spawn(systemsInstance);
            }

            // 4Spawn points
            if (spawnPointPrefab != null)
                SpawnPlayerSpawnPoints();

            yield return WaitForPhysicsReady();

            // Кастомные объекты
            if (customPrefab != null)
                SpawnCustomPrefab();

            Debug.Log("[WorldGen] Generation complete");

            // Теперь мир реально готов
            OnWorldReady?.Invoke(WorldSession.WorldVersion);
        }

        private IEnumerator WaitForPhysicsReady()
        {
            Debug.Log("[WorldGen] Waiting for physics readiness...");

            int safety = 0;

            while (safety < 50) // максимум ~50 кадров
            {
                Vector3 testPoint = GetWorldCenterSpawn();

                if (Physics.Raycast(
                        testPoint + Vector3.up * 10f,
                        Vector3.down,
                        50f))
                {
                    Debug.Log("[WorldGen] Physics ready");
                    yield break;
                }

                safety++;
                yield return new WaitForFixedUpdate();
            }

            Debug.LogWarning("[WorldGen] Physics readiness timeout!");
        }

        private void Update()
        {
            if (!IsServer || manager == null)
                return;

            manager.UpdateChunks(Vector3.zero, loadDistance, unloadDistance);
            manager.ProcessLoadQueue();
        }

        // ======================================================
        // HELPERS
        // ======================================================

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
                Quaternion rot = Quaternion.identity;

                if (Physics.Raycast(origin, Vector3.down, out var hit, spawnHeightCheck * 2f))
                    pos = hit.point + Vector3.up * 1.5f;

                var sp = Instantiate(spawnPointPrefab, pos, rot);
                sp.name = $"WorldSpawnPoint_{i}";
            }

            Debug.Log($"[WorldGen] Spawned {spawnPointCount} spawn points");
        }
    }
}
