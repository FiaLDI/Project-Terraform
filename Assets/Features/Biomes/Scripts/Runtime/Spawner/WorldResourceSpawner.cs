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

        int totalCount = Mathf.RoundToInt(chunkSize * chunkSize * biome.resourceDensity);
        if (totalCount <= 0) return;

        System.Random prng = new System.Random(coord.x * 19349663 ^ coord.y * 83492791);

        for (int i = 0; i < totalCount; i++)
        {
            ResourceEntry entry = GetWeightedRandom(biome.possibleResources, prng);
            if (entry == null || entry.resourcePrefab == null) continue;

            if (prng.NextDouble() > entry.spawnChance)
                continue;

            float px = coord.x * chunkSize + (float)prng.NextDouble() * chunkSize;
            float pz = coord.y * chunkSize + (float)prng.NextDouble() * chunkSize;

            float h = BiomeHeightUtility.GetHeight(biome, px, pz);
            Vector3 pos = new Vector3(px, h + biome.resourceSpawnYOffset, pz);
            Quaternion rot = Quaternion.Euler(0f, (float)prng.NextDouble() * 360f, 0f);

            GameObject obj = Object.Instantiate(entry.resourcePrefab, pos, rot, parent);
            obj.name = entry.resourcePrefab.name;
        }
    }

    private ResourceEntry GetWeightedRandom(ResourceEntry[] arr, System.Random prng)
    {
        float total = 0f;
        foreach (var r in arr)
            total += Mathf.Max(0.01f, r.weight);

        float v = (float)prng.NextDouble() * total;
        float sum = 0f;

        foreach (var r in arr)
        {
            sum += Mathf.Max(0.01f, r.weight);
            if (sum >= v)
                return r;
        }

        return arr.Length > 0 ? arr[0] : null;
    }
}
