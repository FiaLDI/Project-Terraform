using UnityEngine;
using System.Collections;
using Unity.Mathematics;
using Features.Biomes.Domain;
using Features.Biomes.Application;
using Features.Player.UI;

namespace Features.Biomes.UnityIntegration
{
    public sealed class RuntimeWorldGenerator : MonoBehaviour
    {
        [Header("World Settings")]
        public WorldConfig worldConfig;

        [Header("Player Prefab (PlayerCore)")]
        public GameObject playerPrefab;

        [Header("Systems Prefab (World-only)")]
        public GameObject systemsPrefab;

        [Header("Custom Prefab")]
        public GameObject customPrefab;

        [Header("Chunk Streaming")]
        public int loadDistance = 5;
        public int unloadDistance = 8;

        [Header("Spawn Settings")]
        public float spawnDistanceForward = 5f;
        public float spawnHeightCheck = 500f;

        private ChunkManager manager;
        private GameObject playerInstance;
        private GameObject systemsInstance;

        public static WorldConfig World { get; private set; }

        // ======================================================
        // LIFECYCLE
        // ======================================================

        private void Start()
        {
            if (worldConfig == null)
            {
                Debug.LogError("[RuntimeWorldGenerator] WorldConfig is NULL!");
                return;
            }

            // 1) Биомные правила
            if (!BiomeRuntimeDatabase.Initialized)
                BiomeRuntimeDatabase.Build(worldConfig);

            // 2) ChunkManager
            manager = new ChunkManager(worldConfig);
            World = worldConfig;

            // 3) Первая генерация вокруг (0,0)
            manager.UpdateChunks(Vector3.zero, loadDistance, unloadDistance);
            manager.ProcessLoadQueue();

            // 4) Асинхронный спавн мира + игрока
            StartCoroutine(SpawnSequence());
        }

        private void Update()
        {
            if (manager == null)
                return;

            Vector3 focusPos = playerInstance != null
                ? playerInstance.transform.position
                : Vector3.zero;

            manager.UpdateChunks(focusPos, loadDistance, unloadDistance);
            manager.ProcessLoadQueue();
        }

        // ======================================================
        // SPAWN SEQUENCE
        // ======================================================

        private IEnumerator SpawnSequence()
        {
            // Ждём готовность стартового чанка (0,0)
            Vector2Int startChunk = new Vector2Int(0, 0);

            while (!ChunkExistsAndReady(startChunk))
                yield return null;

            // Пара кадров на физику
            yield return null;
            yield return null;

            // 1) World systems (ТОЛЬКО world)
            if (systemsPrefab != null)
            {
                SpawnSystemsAtCenter();
                yield return null;
            }

            // 2) PlayerCore
            Vector3 spawnPos = GetSafePlayerSpawnPosition();
            Debug.Log("[RuntimeWorldGenerator] Player spawn at: " + spawnPos);

            playerInstance = Instantiate(playerPrefab, spawnPos, Quaternion.identity);

            // 🔑 ЯВНЫЙ БИНДИНГ
            if (LocalPlayerController.I != null)
                LocalPlayerController.I.Bind(playerInstance);

            if (PlayerUIRoot.I != null)
                PlayerUIRoot.I.Bind(playerInstance);

            // для врагов / спавнеров
            if (InstancedSpawnerSystem.Instance != null)
                InstancedSpawnerSystem.Instance.targetOverride = playerInstance.transform;

            yield return null;

            // 3) Кастомный префаб (опционально)
            if (customPrefab != null)
                SpawnCustomPrefabNearPlayer();
        }

        // ======================================================
        // SPAWN HELPERS
        // ======================================================

        /// <summary>
        /// Находит безопасную позицию для спавна игрока
        /// </summary>
        private Vector3 GetSafePlayerSpawnPosition()
        {
            int cs = worldConfig.chunkSize;

            float centerX = cs * 0.5f;
            float centerZ = cs * 0.5f;

            Vector3 origin = new Vector3(centerX, spawnHeightCheck, centerZ);

            if (Physics.Raycast(origin, Vector3.down, out var hit, spawnHeightCheck * 2f))
            {
                Debug.Log("[Spawn] Raycast hit at " + hit.point);
                return hit.point + Vector3.up * 2f;
            }

            Debug.LogWarning("[Spawn] Raycast failed, fallback to height map");

            float h = worldConfig.GetHeight(new float2(centerX, centerZ));
            return new Vector3(centerX, h + 2f, centerZ);
        }

        private void SpawnSystemsAtCenter()
        {
            Vector3 pos = GetSafePlayerSpawnPosition();

            systemsInstance = Instantiate(
                systemsPrefab,
                pos,
                Quaternion.identity
            );

            systemsInstance.name = "GameSystems";
        }

        private void SpawnCustomPrefabNearPlayer()
        {
            if (customPrefab == null || playerInstance == null)
                return;

            Vector3 startPos = playerInstance.transform.position +
                               playerInstance.transform.forward * 3f +
                               Vector3.up * 50f;

            if (GroundSnapUtility.TrySnapWithNormal(
                    startPos,
                    out Vector3 snapped,
                    out Quaternion rot,
                    out _))
            {
                Instantiate(customPrefab, snapped, rot);
            }
        }

        private bool ChunkExistsAndReady(Vector2Int c)
        {
            string name = $"Chunk_{c.x}_{c.y}";
            var chunkGO = GameObject.Find(name);

            if (chunkGO == null)
                return false;

            return chunkGO.GetComponentInChildren<MeshCollider>() != null;
        }
    }
}
