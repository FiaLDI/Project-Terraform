using UnityEngine;
using System.Collections.Generic;

public class EnemyChunkSpawner
{
    private readonly Vector2Int coord;
    private readonly int chunkSize;
    private readonly BiomeConfig biome;
    private readonly Transform parent;

    private readonly List<Blocker> blockers;

    private System.Random rng;

    public EnemyChunkSpawner(Vector2Int coord, int chunkSize, BiomeConfig biome, Transform parent, List<Blocker> blockers)
    {
        this.coord = coord;
        this.chunkSize = chunkSize;
        this.biome = biome;
        this.parent = parent;
        this.blockers = blockers;

        rng = new System.Random(coord.x * 1337 + coord.y * 7331);
    }

    public void Spawn()
    {
        if (biome.enemyTable == null || biome.enemyTable.Length == 0)
            return;

        int count = Mathf.RoundToInt(chunkSize * chunkSize * biome.enemyDensity);

        for (int i = 0; i < count; i++)
        {
            EnemySpawnEntry entry = SelectWeightedEnemy();
            if (entry == null) continue;

            if (rng.NextDouble() > entry.spawnChance) continue;

            int group = rng.Next(entry.minGroup, entry.maxGroup + 1);

            for (int g = 0; g < group; g++)
                TrySpawnOne(entry);
        }
    }

    private void TrySpawnOne(EnemySpawnEntry entry)
    {
        float px = coord.x * chunkSize + (float)rng.NextDouble() * chunkSize;
        float pz = coord.y * chunkSize + (float)rng.NextDouble() * chunkSize;

        if (!GroundSnapUtility.TrySnapWithNormal(
            new Vector3(px, 50f, pz),
            out Vector3 pos,
            out Quaternion normal,
            out float slope))
        {
            return;
        }

        // SLOPE / HEIGHT RULES
        if (slope < entry.minSlope || slope > entry.maxSlope)
            return;

        float height = pos.y;
        if (height < entry.minHeight || height > entry.maxHeight)
            return;

        if (blockers != null)
        {
            foreach (var b in blockers)
            {
                if (Vector3.Distance(pos, b.position) < b.radius)
                    return;
            }
        }

        var obj = Object.Instantiate(entry.prefab, pos, normal, parent);
    }

    private EnemySpawnEntry SelectWeightedEnemy()
    {
        float total = 0f;
        foreach (var e in biome.enemyTable)
            total += e.weight;

        if (total <= 0f) return null;

        float r = (float)rng.NextDouble() * total;

        foreach (var e in biome.enemyTable)
        {
            if (r < e.weight)
                return e;
            r -= e.weight;
        }

        return null;
    }
}
