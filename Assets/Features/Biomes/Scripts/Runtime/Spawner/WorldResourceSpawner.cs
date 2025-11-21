using UnityEngine;

public class WorldResourceSpawner
{
    private readonly Vector2Int coord;
    private readonly int chunkSize;
    private readonly BiomeConfig biome;
    private readonly Transform parent;

    public WorldResourceSpawner(Vector2Int coord, int chunkSize, BiomeConfig biome, Transform parent)
    {
        this.coord = coord;
        this.chunkSize = chunkSize;
        this.biome = biome;
        this.parent = parent;
    }

    public void Spawn()
    {
        if (biome.possibleResources == null || biome.possibleResources.Length == 0)
            return;

        System.Random prng = new System.Random(coord.x * 928371 + coord.y * 527);

        int groups = Mathf.RoundToInt(chunkSize * chunkSize * biome.resourceDensity);

        for (int i = 0; i < groups; i++)
        {
            ResourceEntry entry = SelectWeightedResource(prng);
            if (entry == null) continue;
            if (prng.NextDouble() > entry.spawnChance) continue;

            float cx = coord.x * chunkSize + (float)prng.NextDouble() * chunkSize;
            float cz = coord.y * chunkSize + (float)prng.NextDouble() * chunkSize;

            float h = BiomeHeightUtility.GetHeight(biome, cx, cz);
            Vector3 center = new Vector3(cx, h, cz);

            int count = prng.Next(entry.clusterCountRange.x, entry.clusterCountRange.y + 1);

            switch (entry.clusterType)
            {
                case ResourceClusterType.Single:
                    SpawnSingle(entry, center);
                    break;

                case ResourceClusterType.CrystalVein:
                    SpawnCrystalVein(prng, entry, center, count);
                    break;

                case ResourceClusterType.RoundCluster:
                    SpawnRoundCluster(prng, entry, center, count);
                    break;

                case ResourceClusterType.VerticalStackNoise:
                    SpawnVerticalNoise(entry, center, count);
                    break;
            }
        }
    }

    private void SpawnSingle(ResourceEntry entry, Vector3 center)
    {
        float y = entry.followTerrain ?
            BiomeHeightUtility.GetHeight(biome, center.x, center.z) :
            center.y;

        Vector3 pos = new Vector3(center.x, y + biome.resourceSpawnYOffset, center.z);
        Object.Instantiate(entry.resourcePrefab, pos, Quaternion.identity, parent);
    }

    private void SpawnCrystalVein(System.Random rng, ResourceEntry entry, Vector3 center, int count)
    {
        for (int i = 0; i < count; i++)
        {
            float ox = ((float)rng.NextDouble() * 2 - 1) * entry.clusterRadius;
            float oz = ((float)rng.NextDouble() * 2 - 1) * entry.clusterRadius;

            float x = center.x + ox;
            float z = center.z + oz;

            float y = entry.followTerrain ?
                BiomeHeightUtility.GetHeight(biome, x, z) :
                center.y;

            Vector3 p = new Vector3(x, y + biome.resourceSpawnYOffset, z);
            Object.Instantiate(entry.resourcePrefab, p, Quaternion.identity, parent);
        }
    }

    private void SpawnRoundCluster(System.Random rng, ResourceEntry entry, Vector3 center, int count)
    {
        for (int i = 0; i < count; i++)
        {
            float angle = (float)rng.NextDouble() * Mathf.PI * 2;
            float dist = (float)rng.NextDouble() * entry.clusterRadius;

            float x = center.x + Mathf.Cos(angle) * dist;
            float z = center.z + Mathf.Sin(angle) * dist;

            float y = entry.followTerrain ?
                BiomeHeightUtility.GetHeight(biome, x, z) :
                center.y;

            Vector3 p = new Vector3(x, y + biome.resourceSpawnYOffset, z);
            Object.Instantiate(entry.resourcePrefab, p, Quaternion.identity, parent);
        }
    }

    private void SpawnVerticalNoise(ResourceEntry entry, Vector3 center, int count)
    {
        float baseY = BiomeHeightUtility.GetHeight(biome, center.x, center.z);

        for (int i = 0; i < count; i++)
        {
            float noiseX = Mathf.PerlinNoise(center.x * entry.noiseScale, i * entry.noiseScale) - 0.5f;
            float noiseZ = Mathf.PerlinNoise(center.z * entry.noiseScale, i * entry.noiseScale + 100) - 0.5f;

            float x = center.x + noiseX * entry.noiseAmplitude;
            float z = center.z + noiseZ * entry.noiseAmplitude;

            float y = entry.followTerrain ?
                BiomeHeightUtility.GetHeight(biome, x, z) :
                baseY + i * entry.verticalStep;

            Vector3 pos = new Vector3(x, y + biome.resourceSpawnYOffset, z);
            Object.Instantiate(entry.resourcePrefab, pos, Quaternion.identity, parent);
        }
    }

    private ResourceEntry SelectWeightedResource(System.Random rng)
    {
        float t = 0;
        foreach (var e in biome.possibleResources) t += e.weight;

        float r = (float)rng.NextDouble() * t;

        foreach (var e in biome.possibleResources)
        {
            if (r < e.weight) return e;
            r -= e.weight;
        }

        return null;
    }
}
