using UnityEngine;
using Features.Biomes.Domain;
using Features.Enemy.Data;
using Features.Pooling;
using Features.Enemy;

public class BiomeEnemySpawner : MonoBehaviour
{
    public Transform player;
    public WorldConfig world;

    private float spawnTimer;

    void LateUpdate()
    {
        if (!player || !world) return;

        BiomeConfig biome = GetDominantBiome();
        if (!biome || biome.enemyTable == null || biome.enemyTable.Length == 0)
            return;

        if (EnemyBiomeCounter.GetCount(biome) >= biome.enemyTable.Length * 12)
            return;

        if (!EnemyWorldManager.Instance.CanSpawn())
            return;

        spawnTimer += Time.deltaTime;
        if (spawnTimer < 0.3f) return;
        spawnTimer = 0f;

        SpawnEnemy(biome);
    }

    private BiomeConfig GetDominantBiome()
    {
        var blend = world.GetBiomeBlend(player.position);

        BiomeConfig best = null;
        float bestWeight = 0f;

        foreach (var b in blend)
        {
            if (b.biome && b.weight > bestWeight)
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

        // --- ПУЛЛИНГ ---
        PoolMeta meta = config.prefab.GetComponent<PoolMeta>();
        GameObject enemyGO;

        if (meta != null)
        {
            // Index must be set in editor or automatically assigned
            PoolObject pooled = SmartPool.Instance.Get(meta.prefabIndex, config.prefab);
            if (pooled == null) return;
            
            enemyGO = pooled.gameObject;

            pooled.transform.position = pos;
            pooled.transform.rotation = Quaternion.identity;
        }
        else
        {
            // Нет PoolMeta? — обычный Instantiate.
            enemyGO = Instantiate(config.prefab, pos, Quaternion.identity);
        }

        // --- Настройка врага ---
        var tracker = enemyGO.GetComponent<EnemyInstanceTracker>();
        if (!tracker)
            tracker = enemyGO.AddComponent<EnemyInstanceTracker>();
        tracker.config = config;

        // EnemyHealth также должен иметь ссылку на config
        var health = enemyGO.GetComponent<EnemyHealth>();
        if (health)
            health.config = config;

        // EnemyLODController тоже
        var lod = enemyGO.GetComponent<EnemyLODController>();
        if (lod)
            lod.config = config;

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
