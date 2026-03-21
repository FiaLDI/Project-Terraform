using Features.Biomes.Domain;
using Features.Enemy;
using Features.Enemy.Data;
using Features.Enemy.UnityIntegration;
using UnityEngine;
using FishNet.Object;
using FishNet;

public class BiomeEnemySpawner : MonoBehaviour
{
    public Transform player;
    public WorldConfig world;

    private float spawnTimer;


    void LateUpdate()
    {
        if (!InstanceFinder.IsServer)
            return;

        if (player == null || world == null)
            return;

        // 1) Получаем биом
        BiomeConfig biome = GetDominantBiome();
        if (biome == null)
            return;

        // 2) Проверяем enemyTable
        var table = biome.enemyTable;
        if (table == null || table.Length == 0)
            return;

        // 3) EnemyWorldManager может быть null!
        if (EnemyWorldManager.Instance == null)
            return;

        // 4) EnemyBiomeCounter может упасть, если biome некорректный
        int currentCount = 0;
        try
        {
            currentCount = EnemyBiomeCounter.GetCount(biome);
        }
        catch
        {
            return;
        }

        if (currentCount >= table.Length * 12)
            return;

        // 5) Проверяем CanSpawn
        if (!EnemyWorldManager.Instance.CanSpawn())
            return;

        // 6) Таймер
        spawnTimer += Time.deltaTime;
        if (spawnTimer < 0.3f)
            return;

        spawnTimer = 0f;

        SpawnEnemy(biome);
    }

    private BiomeConfig GetDominantBiome()
    {
        if (world == null || player == null)
            return null;

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

    private void SpawnEnemy(BiomeConfig biome)
    {
        var entry = biome.enemyTable[Random.Range(0, biome.enemyTable.Length)];

        EnemyConfigSO config = entry.config;
        if (!config || !config.prefab)
            return;

        Vector3 pos = GetSpawnPosition();

        // ❗ ВАЖНО: только Instantiate (без пула)
        GameObject enemyGO = Instantiate(config.prefab, pos, Quaternion.identity);

        // --- ДО Spawn: настраиваем критичные данные ---
        var tracker = enemyGO.GetComponent<EnemyInstanceTracker>();
        if (!tracker)
            tracker = enemyGO.AddComponent<EnemyInstanceTracker>();

        tracker.config = config;

        var lod = enemyGO.GetComponent<EnemyLODController>();
        if (lod)
            lod.config = config;
        
        var binder = enemyGO.GetComponent<EnemyEcsRuntimeBinder>();
        if (binder != null)
            binder.SetConfig(config);

        // --- FishNet Spawn ---
        var nob = enemyGO.GetComponent<NetworkObject>();
        if (nob == null)
        {
            Debug.LogError("No NetworkObject on enemy prefab!");
            Destroy(enemyGO);
            return;
        }

        InstanceFinder.ServerManager.Spawn(nob);

        // --- ПОСЛЕ Spawn ---
        var link = enemyGO.GetComponent<EnemyChunkLink>();
        if (!link)
            link = enemyGO.AddComponent<EnemyChunkLink>();

        link.chunkCoord = world.WorldToChunk(pos);

        // --- Регистрация ---
        EnemyWorldManager.Instance.Register(tracker);
        EnemyBiomeCounter.Register(biome, tracker);

        // --- Auto-unregister ---
        var unreg = enemyGO.GetComponent<EnemyAutoUnregister>();
        if (!unreg)
            unreg = enemyGO.AddComponent<EnemyAutoUnregister>();

        unreg.biome = biome;
        unreg.tracker = tracker;
    }

    private Vector3 GetSpawnPosition()
    {
        float r = Random.Range(12f, 40f);
        Vector2 circle = Random.insideUnitCircle.normalized * r;
        return player.position + new Vector3(circle.x, 0, circle.y);
    }
}
