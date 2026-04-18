using System.Collections.Generic;
using UnityEngine;
using FishNet;
using FishNet.Object;
using FishNet.Connection;
using Biomes.Data;
using Biomes.Application;
using Features.Player.UnityIntegration;

namespace Biomes.UnityIntegration
{
    public sealed class BiomeEnemySpawner : NetworkBehaviour
    {
        private sealed class PlayerSpawnState
        {
            public readonly List<EnemyInstanceTracker> Enemies = new();
            public float NextSpawnTime;
        }

        [Header("Config")]
        [SerializeField] private WorldConfig world;

        [Header("Spawn Settings")]
        [SerializeField] private float spawnInterval = 1.2f;
        [SerializeField] private float spawnRadiusMin = 20f;
        [SerializeField] private float spawnRadiusMax = 40f;
        [SerializeField] private float despawnDistance = 80f;

        [Header("Limits")]
        [SerializeField] private int maxPerPlayer = 12;
        [SerializeField] private int maxPerBiome = 24;
        [SerializeField] private int maxGlobal = 120;

        private float nextUpdateTime;
        private readonly Dictionary<int, PlayerSpawnState> playerEnemies = new();

        private void Update()
        {
            if (!IsServer)
                return;

            if (Time.time < nextUpdateTime)
                return;

            if (ServerCompositionRoot.I != null &&
                ServerCompositionRoot.I.CurrentWorldType == WorldType.Static)
            {
                return;
            }

            nextUpdateTime = Time.time + spawnInterval;

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

        private void HandlePlayerSpawn(NetworkConnection client, Transform player)
        {
            var activeWorld = RuntimeWorldGenerator.World != null
                ? RuntimeWorldGenerator.World
                : world;

            if (activeWorld == null)
                return;

            int id = client.ClientId;

            if (!playerEnemies.TryGetValue(id, out var state))
            {
                state = new PlayerSpawnState();
                playerEnemies[id] = state;
            }

            CleanupEnemyList(state.Enemies);

            if (Time.time < state.NextSpawnTime)
                return;

            if (state.Enemies.Count >= GetScaledLimit(maxPerPlayer))
            {
                state.NextSpawnTime = Time.time + spawnInterval;
                return;
            }

            if (!CanSpawnGlobally())
            {
                state.NextSpawnTime = Time.time + spawnInterval;
                return;
            }

            var biome = GetDominantBiome(activeWorld, player.position);
            if (biome == null)
            {
                state.NextSpawnTime = Time.time + spawnInterval;
                return;
            }

            if (EnemyBiomeCounter.GetCountSafe(biome) >= GetScaledLimit(maxPerBiome))
            {
                state.NextSpawnTime = Time.time + GetRespawnDelay(biome);
                return;
            }

            if (!TryGetSpawnHit(player, out var hit))
            {
                state.NextSpawnTime = Time.time + spawnInterval;
                return;
            }

            var entry = SelectEnemyEntry(biome, hit);
            if (entry == null)
            {
                state.NextSpawnTime = Time.time + GetRespawnDelay(biome);
                return;
            }

            bool spawned = SpawnEnemy(client, biome, entry, hit.point, state.Enemies);
            state.NextSpawnTime = Time.time + (spawned ? GetRespawnDelay(biome) : spawnInterval);
        }

        private bool SpawnEnemy(
            NetworkConnection owner,
            BiomeConfig biome,
            EnemySpawnEntry entry,
            Vector3 pos,
            List<EnemyInstanceTracker> list)
        {
            var config = entry.config;

            if (!config || !config.prefab)
                return false;

            var go = Instantiate(config.prefab, pos, Quaternion.identity);

            var binder = go.GetComponent<EnemyEcsRuntimeBinder>();
            if (binder != null)
            {
                binder.SetConfig(config);
                binder.SetDespawnDistance(despawnDistance);
            }

            var rb = go.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.position = pos;
                rb.rotation = Quaternion.identity;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            var nob = go.GetComponent<NetworkObject>();
            if (!nob)
            {
                Destroy(go);
                return false;
            }

            InstanceFinder.ServerManager.Spawn(nob);

            var tracker = go.GetComponent<EnemyInstanceTracker>() ?? go.AddComponent<EnemyInstanceTracker>();
            tracker.config = config;

            if (go.GetComponent<EnemyDespawnBridge>() == null)
                go.AddComponent<EnemyDespawnBridge>();

            var autoUnregister = go.GetComponent<EnemyAutoUnregister>() ?? go.AddComponent<EnemyAutoUnregister>();
            autoUnregister.biome = biome;
            autoUnregister.tracker = tracker;

            list.Add(tracker);
            EnemyWorldManager.Instance?.Register(tracker);
            EnemyBiomeCounter.Register(biome, tracker);
            return true;
        }

        private bool TryGetSpawnHit(Transform player, out RaycastHit result)
        {
            for (int i = 0; i < 8; i++)
            {
                float r = Random.Range(spawnRadiusMin, spawnRadiusMax);
                Vector2 dir = Random.insideUnitCircle.normalized * r;

                Vector3 pos = player.position + new Vector3(dir.x, 0f, dir.y);

                Vector3 dirToSpawn = (pos - player.position).normalized;
                if (Vector3.Dot(player.forward, dirToSpawn) > 0.6f)
                    continue;

                if (Physics.Raycast(
                    pos + Vector3.up * 50f,
                    Vector3.down,
                    out var hit,
                    100f,
                    ~0,
                    QueryTriggerInteraction.Ignore))
                {
                    result = hit;
                    return true;
                }
            }

            result = default;
            return false;
        }

        private BiomeConfig GetDominantBiome(WorldConfig activeWorld, Vector3 pos)
        {
            var blend = activeWorld.GetBiomeBlend(pos);
            if (blend == null || blend.Length == 0)
                return null;

            BiomeConfig best = null;
            float bestWeight = 0f;

            foreach (var b in blend)
            {
                if (b.biome == null)
                    continue;

                if (b.weight > bestWeight)
                {
                    best = b.biome;
                    bestWeight = b.weight;
                }
            }

            return best;
        }

        public override void OnStopServer()
        {
            playerEnemies.Clear();
        }

        private void CleanupEnemyList(List<EnemyInstanceTracker> list)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var enemy = list[i];
                if (enemy == null || !enemy.gameObject.activeInHierarchy)
                    list.RemoveAt(i);
            }
        }

        private bool CanSpawnGlobally()
        {
            if (EnemyWorldManager.Instance != null)
                return EnemyWorldManager.Instance.CanSpawn(maxGlobal);

            return EnemyInstanceTracker.All.Count < GetScaledLimit(maxGlobal);
        }

        private int GetScaledLimit(int baseLimit)
        {
            float scale = EnemyPerformanceManager.Instance != null
                ? EnemyPerformanceManager.Instance.EnemyCountScale
                : 1f;

            return Mathf.Max(1, Mathf.RoundToInt(baseLimit * scale));
        }

        private float GetRespawnDelay(BiomeConfig biome)
        {
            float baseDelay = Mathf.Max(spawnInterval, biome != null ? biome.enemyRespawnDelay : spawnInterval);
            float scale = EnemyPerformanceManager.Instance != null
                ? Mathf.Max(0.25f, EnemyPerformanceManager.Instance.EnemyCountScale)
                : 1f;

            return baseDelay / scale;
        }

        private EnemySpawnEntry SelectEnemyEntry(BiomeConfig biome, RaycastHit hit)
        {
            var table = biome.enemyTable;
            if (table == null || table.Length == 0)
                return null;

            float slope = Vector3.Angle(hit.normal, Vector3.up);
            float height = hit.point.y;
            float totalWeight = 0f;

            for (int i = 0; i < table.Length; i++)
            {
                var entry = table[i];
                if (!IsEntryEligible(entry, slope, height))
                    continue;

                float effectiveWeight = entry.weight * Mathf.Clamp01(entry.spawnChance);
                if (effectiveWeight <= 0f)
                    continue;

                totalWeight += effectiveWeight;
            }

            if (totalWeight <= 0f)
                return null;

            float pick = Random.value * totalWeight;

            for (int i = 0; i < table.Length; i++)
            {
                var entry = table[i];
                if (!IsEntryEligible(entry, slope, height))
                    continue;

                float effectiveWeight = entry.weight * Mathf.Clamp01(entry.spawnChance);
                if (effectiveWeight <= 0f)
                    continue;

                pick -= effectiveWeight;
                if (pick <= 0f)
                    return entry;
            }

            return null;
        }

        private static bool IsEntryEligible(EnemySpawnEntry entry, float slope, float height)
        {
            if (entry == null || entry.config == null || entry.config.prefab == null)
                return false;

            if (slope < entry.minSlope || slope > entry.maxSlope)
                return false;

            if (height < entry.minHeight || height > entry.maxHeight)
                return false;

            return true;
        }
    }
}
