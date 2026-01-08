using FishNet.Object;
using UnityEngine;
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

        [SerializeField, Min(1)]
        private int spawnPointCount = 4;

        [SerializeField]
        private float spawnRadius = 15f;

        [Header("Custom Prefab")]
        public GameObject customPrefab;

        [Header("Chunk Streaming")]
        public int loadDistance = 5;
        public int unloadDistance = 8;

        [Header("Spawn Settings")]
        public float spawnHeightCheck = 500f;

        private ChunkManager manager;
        private GameObject systemsInstance;

        public static WorldConfig World { get; private set; }

        public static event System.Action<int> OnWorldReady;


        // ======================================================
        // SERVER
        // ======================================================

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

        private IEnumerator ServerGenerateWorld()
        {
            // 1️⃣ Биомы
            if (!BiomeRuntimeDatabase.Initialized)
                BiomeRuntimeDatabase.Build(worldConfig);

            // 2️⃣ ChunkManager
            manager = new ChunkManager(worldConfig);
            World = worldConfig;

            // 3️⃣ Стартовая генерация чанков
            manager.UpdateChunks(Vector3.zero, loadDistance, unloadDistance);
            manager.ProcessLoadQueue();

            // 🔑 КЛЮЧ: НЕ ждём MeshCollider / физику
            // Даём PhysX 1 тик, чтобы мир начал существовать
            yield return new WaitForFixedUpdate();

            // 4️⃣ Системы мира
            if (systemsPrefab != null)
            {
                systemsInstance = Instantiate(
                    systemsPrefab,
                    GetWorldCenterSpawn(),
                    Quaternion.identity
                );

                Spawn(systemsInstance);
            }

            // 5️⃣ Spawn-point’ы игроков (логические)
            if (spawnPointPrefab != null)
                SpawnPlayerSpawnPoints();

            // 6️⃣ Кастомные объекты
            if (customPrefab != null)
                SpawnCustomPrefab();

            OnWorldReady?.Invoke(WorldSession.WorldVersion);
            Debug.Log("[WorldGen] World generation completed");
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

                // Лёгкий raycast — только если физика уже есть
                if (Physics.Raycast(origin, Vector3.down, out var hit, spawnHeightCheck * 2f))
                    pos = hit.point + Vector3.up * 1.5f;

                var sp = Instantiate(spawnPointPrefab, pos, rot);
                sp.name = $"WorldSpawnPoint_{i}";
            }

            Debug.Log($"[WorldGen] Spawned {spawnPointCount} player spawn points");
        }
    }
}
