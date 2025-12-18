using UnityEngine;
using System.Collections;
using Unity.Mathematics;
using Features.Biomes.Domain;
using Features.Biomes.Application;

namespace Features.Biomes.UnityIntegration
{
    public class RuntimeWorldGenerator : MonoBehaviour
    {
        [Header("World Settings")]
        public WorldConfig worldConfig;

        [Header("Player Prefab")]
        public GameObject playerPrefab;

        [Header("Systems Prefab")]
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

        private bool worldReady = false;

        public static GameObject PlayerInstance { get; private set; }
        public static WorldConfig World { get; private set; }

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

            // 4) Асинхронный спавн
            StartCoroutine(SpawnSequence());
            manager.ProcessLoadQueue();
        }

        private void Update()
        {
            if (manager == null)
                return;

            Vector3 focusPos;

            if (playerInstance != null)
                focusPos = playerInstance.transform.position;
            else
                focusPos = Vector3.zero;

            manager.UpdateChunks(
                focusPos,
                loadDistance,
                unloadDistance
            );

            manager.ProcessLoadQueue();
        }

        private IEnumerator SpawnSequence()
        {
            // Чанк, в котором хотим заспавнить игрока — (0,0)
            Vector2Int playerChunk = new Vector2Int(0, 0);

            // Ждём, пока этот чанк реально появится и на нём будет MeshCollider
            while (!ChunkExistsAndReady(playerChunk))
                yield return null;

            // ещё пара кадров на обновление физики
            yield return null;
            yield return null;

            // 1) SystemsPrefab
            if (systemsPrefab != null)
            {
                SpawnSystemsAtCenter();
                yield return null;
            }

            // 2) Игрок
            Vector3 spawnPos = GetSafePlayerSpawnPosition();
            Debug.Log("[RuntimeWorldGenerator] Player spawn at: " + spawnPos);

            playerInstance = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
            PlayerInstance = playerInstance;

            if (InstancedSpawnerSystem.Instance != null)
                InstancedSpawnerSystem.Instance.targetOverride = playerInstance.transform;

            yield return null;

            // 3) Кастомный префаб
            if (customPrefab != null)
                SpawnCustomPrefabNearPlayer();

            worldReady = true;
        }


        /// <summary>
        /// Находит безопасную позицию для спавна:
        /// - несколько Raycast'ов сверху в разных точках;
        /// - если все мимо, fallback по GetHeight.
        /// </summary>
        private Vector3 GetSafePlayerSpawnPosition()
        {
            int cs = worldConfig.chunkSize;

            // центр первого чанка в мировых координатах
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


        private void SpawnSystemsInFront()
        {
            if (playerInstance == null || systemsPrefab == null)
                return;

            Vector3 forwardPos = playerInstance.transform.position +
                                 playerInstance.transform.forward * spawnDistanceForward +
                                 Vector3.up * spawnHeightCheck;

            if (Physics.Raycast(forwardPos, Vector3.down, out var hit, spawnHeightCheck * 2f))
            {
                systemsInstance = Instantiate(
                    systemsPrefab,
                    hit.point + Vector3.up * 1f,
                    Quaternion.identity
                );
                systemsInstance.name = "GameSystems";
            }
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
                    out float slope))
            {
                GameObject inst = Instantiate(customPrefab, snapped, rot);
                inst.name = "CustomPrefab";

                foreach (Transform child in inst.transform)
                {
                    Vector3 childStart = child.position + Vector3.up * 20f;

                    if (GroundSnapUtility.TrySnapWithNormal(
                            childStart,
                            out Vector3 cPos,
                            out Quaternion cRot,
                            out float cSlope))
                    {
                        child.position = cPos;
                        child.rotation = cRot * Quaternion.Euler(0, child.rotation.eulerAngles.y, 0);
                    }
                }
            }
        }

        private void SpawnSystemsAtCenter()
        {
            if (systemsPrefab == null)
                return;

            Vector3 pos = GetSafePlayerSpawnPosition();

            systemsInstance = Instantiate(
                systemsPrefab,
                pos,
                Quaternion.identity
            );
            systemsInstance.name = "GameSystems";
        }

        private bool ChunkExistsAndReady(Vector2Int c)
        {
            // Ищем объект чанка в сцене
            string name = $"Chunk_{c.x}_{c.y}";
            var chunkGO = GameObject.Find(name);
            if (chunkGO == null)
                return false;

            // Collider должен уже создаться
            return chunkGO.GetComponentInChildren<MeshCollider>() != null;
        }

    }
}
