using UnityEngine;
using Features.Biomes.Domain;
using Features.Enemy.Data;
using Features.Pooling;
using Features.Enemy;
using Features.Enemy.UnityIntegration;

public class BiomeEnemySpawner : MonoBehaviour
{
    public Transform player;
    public WorldConfig world;

    private float spawnTimer;

    void LateUpdate()
    {
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
        //var health = enemyGO.GetComponent<EnemyHealth>();
        //if (health)
        //    health.config = config;

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
