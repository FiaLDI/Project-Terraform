using Features.Enemy.Data;
using UnityEngine;
using FishNet;
using FishNet.Object;
using Biomes.Data;
using Biomes.Application;

namespace Biomes.UnityIntegration
{
    public class BiomeEnemySpawner : MonoBehaviour
    {
        public Transform player;
        public WorldConfig world;

        [Header("Spawn Settings")]
        [SerializeField] private float spawnInterval = 0.4f;
        [SerializeField] private float spawnRadiusMin = 15f;
        [SerializeField] private float spawnRadiusMax = 35f;
        [SerializeField] private int maxEnemies = 40;

        private float nextSpawnTime;

        // =========================================================
        private void Update()
        {
            if (!InstanceFinder.IsServer)
                return;

            if (Time.time < nextSpawnTime)
                return;

            if (player == null || world == null)
                return;

            var biome = GetDominantBiome();
            if (biome == null)
                return;

            if (!CanSpawn(biome))
                return;

            nextSpawnTime = Time.time + spawnInterval;

            SpawnEnemy(biome);
        }

        // =========================================================
        private bool CanSpawn(BiomeConfig biome)
        {
            if (EnemyWorldManager.Instance == null)
                return false;

            int count = EnemyBiomeCounter.GetCount(biome);

            if (count >= maxEnemies)
                return false;

            return EnemyWorldManager.Instance.CanSpawn();
        }

        // =========================================================
        private void SpawnEnemy(BiomeConfig biome)
        {
            var entry = biome.enemyTable[Random.Range(0, biome.enemyTable.Length)];
            var config = entry.config;

            if (!config || !config.prefab)
                return;

            if (!TryGetSpawnPosition(out Vector3 pos))
                return;

            GameObject enemyGO = Instantiate(config.prefab, pos, Quaternion.identity);

            // 🔥 СНАЧАЛА ECS
            var binder = enemyGO.GetComponent<EnemyEcsRuntimeBinder>();
            if (binder != null)
            {
                binder.SetConfig(config);
                binder.ForceInit();
            }

            // 🔥 ПОТОМ СЕТЬ
            var nob = enemyGO.GetComponent<NetworkObject>();
            if (!nob)
            {
                Debug.LogError("Enemy prefab has no NetworkObject!");
                Destroy(enemyGO);
                return;
            }

            InstanceFinder.ServerManager.Spawn(nob);

            Register(enemyGO, biome, config, pos);
        }

        // =========================================================
        private void Register(GameObject enemyGO, BiomeConfig biome, EnemyConfigSO config, Vector3 pos)
        {
            var tracker = enemyGO.GetComponent<EnemyInstanceTracker>() 
                          ?? enemyGO.AddComponent<EnemyInstanceTracker>();

            tracker.config = config;

            var link = enemyGO.GetComponent<EnemyChunkLink>() 
                       ?? enemyGO.AddComponent<EnemyChunkLink>();

            link.chunkCoord = world.WorldToChunk(pos);

            EnemyWorldManager.Instance.Register(tracker);
            EnemyBiomeCounter.Register(biome, tracker);

            var unreg = enemyGO.GetComponent<EnemyAutoUnregister>() 
                        ?? enemyGO.AddComponent<EnemyAutoUnregister>();

            unreg.biome = biome;
            unreg.tracker = tracker;
        }

        // =========================================================
        private bool TryGetSpawnPosition(out Vector3 result)
        {
            for (int i = 0; i < 10; i++)
            {
                float r = Random.Range(spawnRadiusMin, spawnRadiusMax);
                Vector2 circle = Random.insideUnitCircle.normalized * r;

                Vector3 pos = player.position + new Vector3(circle.x, 0, circle.y);

                if (Vector3.Distance(player.position, pos) < spawnRadiusMin)
                    continue;

                result = pos;
                return true;
            }

            result = default;
            return false;
        }

        // =========================================================
        private BiomeConfig GetDominantBiome()
        {
            var blend = world.GetBiomeBlend(player.position);
            if (blend == null || blend.Length == 0)
                return null;

            BiomeConfig best = null;
            float bestWeight = 0f;

            foreach (var b in blend)
            {
                if (b.biome == null) continue;

                if (b.weight > bestWeight)
                {
                    best = b.biome;
                    bestWeight = b.weight;
                }
            }

            return best;
        }
    }
}
