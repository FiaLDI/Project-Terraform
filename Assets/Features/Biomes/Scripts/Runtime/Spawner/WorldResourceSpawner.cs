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

        int count = Mathf.RoundToInt(chunkSize * chunkSize * biome.resourceDensity);

        for (int i = 0; i < count; i++)
        {
            ResourceEntry entry = SelectWeightedResource(prng);
            if (entry == null) continue;

            if (prng.NextDouble() > entry.spawnChance)
                continue;

            float px = coord.x * chunkSize + (float)prng.NextDouble() * chunkSize;
            float pz = coord.y * chunkSize + (float)prng.NextDouble() * chunkSize;

            float h = BiomeHeightUtility.GetHeight(biome, px, pz);
            Vector3 pos = new Vector3(px, h + biome.resourceSpawnYOffset, pz);

            Object.Instantiate(entry.resourcePrefab, pos, Quaternion.identity, parent);
        }
    }

    private ResourceEntry SelectWeightedResource(System.Random rng)
    {
        float total = 0;
        foreach (var e in biome.possibleResources)
            total += e.weight;

        float r = (float)rng.NextDouble() * total;

        foreach (var e in biome.possibleResources)
        {
            if (r < e.weight)
                return e;
            r -= e.weight;
        }

        return null;
    }
}
