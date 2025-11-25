using UnityEngine;
using System.Collections.Generic;

public class EnvironmentChunkSpawner
{
    private readonly Vector2Int coord;
    private readonly int chunkSize;
    private readonly BiomeConfig biome;
    private readonly Transform parent;
    private readonly List<Blocker> blockers;

    public EnvironmentChunkSpawner(
        Vector2Int coord,
        int chunkSize,
        BiomeConfig biome,
        Transform parent,
        List<Blocker> blockers)
    {
        this.coord = coord;
        this.chunkSize = chunkSize;
        this.biome = biome;
        this.parent = parent;
        this.blockers = blockers;
    }

    public void Spawn()
    {
        if (biome.environmentPrefabs == null || biome.environmentPrefabs.Length == 0)
            return;

        System.Random prng = new System.Random(coord.x * 727 + coord.y * 947);

        int count = Mathf.RoundToInt(chunkSize * chunkSize * biome.environmentDensity);

        for (int i = 0; i < count; i++)
        {
            EnvironmentEntry selected = SelectWeightedEnvironment(prng);
            if (selected == null) continue;

            if (prng.NextDouble() > selected.spawnChance) continue;

            float px = coord.x * chunkSize + (float)prng.NextDouble() * chunkSize;
            float pz = coord.y * chunkSize + (float)prng.NextDouble() * chunkSize;

            float h = BiomeHeightUtility.GetHeight(biome, px, pz);
            Vector3 startPos = new Vector3(px, h + 50f, pz);

            if (!GroundSnapUtility.TrySnapWithNormal(
                    startPos,
                    out Vector3 groundPos,
                    out Quaternion normalRot,
                    out float slope))
                continue;

            if (slope < selected.minSlope || slope > selected.maxSlope)
                continue;

            Quaternion finalRot = Quaternion.identity;

            if (selected.alignToNormal)
                finalRot = normalRot;

            if (selected.randomYRotation)
            {
                float yaw = (float)prng.NextDouble() * 360f;
                Quaternion yRot = Quaternion.Euler(0f, yaw, 0f);
                finalRot *= yRot;
            }

            float scale = 1f;
            if (selected.randomScale)
            {
                double t = prng.NextDouble();
                scale = Mathf.Lerp(selected.minScale, selected.maxScale, (float)t);
            }

            GameObject obj = Object.Instantiate(selected.prefab, groundPos, finalRot, parent);
            obj.transform.localScale *= scale;

            // крупные объекты помечаем как блокеры для ресурсов/врагов
            if (selected.markAsResourceBlocker)
            {
                // радиус можно подправить, сейчас условные 1.5f
                blockers.Add(new Blocker(groundPos, 1.5f));
            }
        }
    }

    private EnvironmentEntry SelectWeightedEnvironment(System.Random rng)
    {
        float total = 0f;
        foreach (var e in biome.environmentPrefabs)
            total += e.weight;

        if (total <= 0f) return null;

        float r = (float)rng.NextDouble() * total;

        foreach (var e in biome.environmentPrefabs)
        {
            if (r < e.weight)
                return e;
            r -= e.weight;
        }

        return null;
    }
}
