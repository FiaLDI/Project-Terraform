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

        [Header("Spawn Point")]
        [SerializeField] private ScenePlayerSpawnPoint spawnPointPrefab;

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
            // 1) Биомы
            if (!BiomeRuntimeDatabase.Initialized)
                BiomeRuntimeDatabase.Build(worldConfig);

            // 2) ChunkManager
            manager = new ChunkManager(worldConfig);
            World = worldConfig;

            // 3) Стартовая генерация
            manager.UpdateChunks(Vector3.zero, loadDistance, unloadDistance);
            manager.ProcessLoadQueue();

            // 4) Ждём стартовый чанк
            while (!ChunkExistsAndReady(Vector2Int.zero))
                yield return null;

            // 5) Системы мира
            if (systemsPrefab != null)
            {
                systemsInstance = Instantiate(
                    systemsPrefab,
                    GetWorldCenterSpawn(),
                    Quaternion.identity
                );

                Spawn(systemsInstance);
            }

            // 6) Spawn point для игроков (КЛЮЧЕВОЕ)
            if (spawnPointPrefab != null)
                SpawnPlayerSpawnPoint();

            // 7) Кастомные объекты
            if (customPrefab != null)
                SpawnCustomPrefab();
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

        private void SpawnPlayerSpawnPoint()
        {
            Vector3 pos = GetWorldCenterSpawn();
            Quaternion rot = Quaternion.identity;

            var sp = Instantiate(spawnPointPrefab, pos, rot);
            sp.name = "WorldSpawnPoint";

            // ❗ NetworkObject НЕ нужен
            // сервер использует его как IPlayerSpawnProvider
        }

        private bool ChunkExistsAndReady(Vector2Int c)
        {
            var go = GameObject.Find($"Chunk_{c.x}_{c.y}");
            return go != null && go.GetComponentInChildren<MeshCollider>() != null;
        }
    }
}
