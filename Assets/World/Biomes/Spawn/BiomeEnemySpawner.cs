using System.Collections.Generic;
using UnityEngine;
using FishNet;
using FishNet.Object;
using FishNet.Connection;
using Biomes.Data;
using Biomes.Application;
using Features.Enemy.Data;
using Features.Player.UnityIntegration;

namespace Biomes.UnityIntegration
{
    public sealed class BiomeEnemySpawner : NetworkBehaviour
    {
        [Header("Config")]
        [SerializeField] private WorldConfig world;

        [Header("Spawn Settings")]
        [SerializeField] private float spawnInterval = 1.2f;
        [SerializeField] private float spawnRadiusMin = 20f;
        [SerializeField] private float spawnRadiusMax = 40f;

        [Header("Limits")]
        [SerializeField] private int maxPerPlayer = 12;
        [SerializeField] private int maxGlobal = 120;

        private float _nextSpawnTime;

        private readonly Dictionary<int, List<EnemyInstanceTracker>> _playerEnemies = new();

        // =========================================================
        private void Update()
        {
            if (!IsServer)
                return;

            if (Time.time < _nextSpawnTime)
                return;

            if (ServerCompositionRoot.I != null &&
                ServerCompositionRoot.I.CurrentWorldType == WorldType.Static)
            {
                return;
            }

            _nextSpawnTime = Time.time + spawnInterval;

            var registry = PlayerRegistry.Instance;
            if (registry == null)
                return;

            var players = registry.Players;

            for (int i = 0; i < players.Count; i++)
            {
                var player = players[i];
                if (player == null)
                    continue;

                var nob = player.GetComponent<NetworkObject>();
                if (nob == null || !nob.IsSpawned)
                    continue;

                var conn = nob.Owner;
                if (conn == null)
                    continue;

                HandlePlayerSpawn(conn, player.transform);
            }
        }

        // =========================================================
        private void HandlePlayerSpawn(NetworkConnection client, Transform player)
        {
            int id = client.ClientId;

            if (!_playerEnemies.TryGetValue(id, out var list))
            {
                list = new List<EnemyInstanceTracker>();
                _playerEnemies[id] = list;
            }

            // чистка
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (list[i] == null || !list[i].gameObject.activeInHierarchy)
                    list.RemoveAt(i);
            }

            if (list.Count >= maxPerPlayer)
                return;

            if (EnemyWorldManager.Instance != null &&
                EnemyWorldManager.Instance.GetCount() >= maxGlobal)
                return;

            var biome = GetDominantBiome(player.position);
            if (biome == null)
                return;

            if (!TryGetSpawnPosition(player, out var pos))
                return;

            SpawnEnemy(client, biome, pos, list);
        }

        // =========================================================
        private void SpawnEnemy(NetworkConnection owner, BiomeConfig biome, Vector3 pos, List<EnemyInstanceTracker> list)
        {
            var table = biome.enemyTable;
            if (table == null || table.Length == 0)
                return;

            var entry = table[Random.Range(0, table.Length)];
            var config = entry.config;

            if (!config || !config.prefab)
                return;

            // ===== Instantiate =====
            var go = Instantiate(config.prefab, pos, Quaternion.identity);

            // ===== ВАЖНО: сначала config =====
            var binder = go.GetComponent<EnemyEcsRuntimeBinder>();
            if (binder != null)
            {
                binder.SetConfig(config);
            }

            // ===== Rigidbody reset (убираем мусор физики) =====
            var rb = go.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.position = pos;
                rb.rotation = Quaternion.identity;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            // ===== Network spawn =====
            var nob = go.GetComponent<NetworkObject>();
            if (!nob)
            {
                Destroy(go);
                return;
            }

            InstanceFinder.ServerManager.Spawn(nob);

            // ❗ НИКАКИХ ForceInit — OnStartServer сам вызовет Init

            // ===== Tracker =====
            var tracker = go.GetComponent<EnemyInstanceTracker>() ?? go.AddComponent<EnemyInstanceTracker>();
            tracker.config = config;

            // ===== Despawn bridge =====
            var despawnBridge = go.GetComponent<EnemyDespawnBridge>()
                                 ?? go.AddComponent<EnemyDespawnBridge>();

            list.Add(tracker);

            if (EnemyWorldManager.Instance != null)
                EnemyWorldManager.Instance.Register(tracker);
        }

        // =========================================================
        private bool TryGetSpawnPosition(Transform player, out Vector3 result)
        {
            for (int i = 0; i < 8; i++)
            {
                float r = Random.Range(spawnRadiusMin, spawnRadiusMax);
                Vector2 dir = Random.insideUnitCircle.normalized * r;

                Vector3 pos = player.position + new Vector3(dir.x, 0, dir.y);

                // не перед игроком
                Vector3 dirToSpawn = (pos - player.position).normalized;
                if (Vector3.Dot(player.forward, dirToSpawn) > 0.6f)
                    continue;

                // земля
                if (Physics.Raycast(pos + Vector3.up * 50f, Vector3.down, out var hit, 100f))
                {
                    result = hit.point;
                    return true;
                }
            }

            result = default;
            return false;
        }

        // =========================================================
        private BiomeConfig GetDominantBiome(Vector3 pos)
        {
            var blend = world.GetBiomeBlend(pos);
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

        // =========================================================
        public override void OnStopServer()
        {
            _playerEnemies.Clear();
        }
    }
}
