using UnityEngine;
using Features.Biomes.Domain;
using Features.Enemies;
using Features.Pooling;
using Features.Biomes.Application;

public class BiomeEnemySpawner : MonoBehaviour
{
    public Transform player;
    public WorldConfig world;

    private float spawnTimer = 0f;

    void LateUpdate()
    {
        if (player == null || world == null)
            return;

        BiomeConfig biome = GetDominantBiome();
        if (biome == null) return;

        // No enemies in this biome
        if (biome.enemyTable == null || biome.enemyTable.Length == 0)
            return;

        // per-biome limit (простой лимит: N врагов на биом)
        if (EnemyBiomeCounter.GetCount(biome) >= biome.enemyTable.Length * 12)
            return;

        // world limit
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
        float bestW = 0f;

        foreach (var b in blend)
        {
            if (b.biome != null && b.weight > bestW)
            {
                best = b.biome;
                bestW = b.weight;
            }
        }

        return best;
    }

    private void SpawnEnemy(BiomeConfig biome)
    {
        // Random enemy type из таблицы биома
        var entry = biome.enemyTable[Random.Range(0, biome.enemyTable.Length)];

        // Берём дефиницию типа с базового префаба
        EnemyDefinition baseDef = entry.prefab.GetComponent<EnemyDefinition>();

        if (baseDef == null)
        {
            Debug.LogError($"[BiomeEnemySpawner] Enemy prefab '{entry.prefab.name}' missing EnemyDefinition!");
            return;
        }

        Vector3 pos = GetSpawnPosition(biome);

        // Выбор LOD-префаба по дистанции
        GameObject prefabToSpawn = SelectLOD(baseDef, pos);
        if (prefabToSpawn == null)
        {
            Debug.LogWarning("[BiomeEnemySpawner] Selected LOD prefab is NULL, skip spawn.");
            return;
        }

        // Спавн (через пул или Instantiate)
        GameObject enemyInstance = SpawnFromPool(prefabToSpawn, pos);
        if (enemyInstance == null) return;

        // Пытаемся взять EnemyDefinition с инстанса (если он есть)
        EnemyDefinition instanceDef = enemyInstance.GetComponent<EnemyDefinition>();
        if (instanceDef == null)
        {
            // Если на LOD-префабе нет EnemyDefinition (что нормально для LOD1/2),
            // считаем логическим "владельцем" базовый def
            instanceDef = baseDef;
        }

        // Регистрация в глобальном менеджере и по биому
        EnemyWorldManager.Instance.Register(instanceDef);
        EnemyBiomeCounter.Register(biome, instanceDef);

        // Автоматический Unregister при уничтожении / деактивации
        var unreg = enemyInstance.AddComponent<EnemyAutoUnregister>();
        unreg.biome = biome;
        unreg.definition = instanceDef;
    }

    private Vector3 GetSpawnPosition(BiomeConfig biome)
    {
        float r = Random.Range(12f, 40f);
        Vector2 c = Random.insideUnitCircle.normalized * r;

        return player.position + new Vector3(c.x, 0, c.y);
    }

    private GameObject SelectLOD(EnemyDefinition def, Vector3 pos)
    {
        float dist = Vector3.Distance(player.position, pos);
        float scale = EnemyPerformanceManager.Instance?.LodScale ?? 1f;

        float lod0 = def.lod0Distance * scale;
        float lod1 = def.lod1Distance * scale;

        if (dist < lod0) return def.prefabLOD0;
        if (dist < lod1) return def.prefabLOD1;
        return def.prefabLOD2;
    }

    private GameObject SpawnFromPool(GameObject prefab, Vector3 pos)
    {
        var meta = prefab.GetComponent<PoolMeta>();
        if (meta == null)
            return Instantiate(prefab, pos, Quaternion.identity);

        var pooled = SmartPool.Instance.Get(meta.prefabIndex, prefab);
        if (pooled == null) return null;

        pooled.transform.position = pos;
        pooled.Pool = SmartPool.Instance;
        return pooled.gameObject;
    }
}
