using UnityEngine;

public class EnvironmentChunkSpawner
{
    private readonly Vector2Int coord;
    private readonly int chunkSize;
    private readonly BiomeConfig biome;
    private readonly Transform parent;

    public EnvironmentChunkSpawner(Vector2Int coord, int chunkSize, BiomeConfig biome, Transform parent)
    {
        this.coord = coord;
        this.chunkSize = chunkSize;
        this.biome = biome;
        this.parent = parent;
    }

    public void Spawn()
    {
        if (biome.environmentPrefabs == null || biome.environmentPrefabs.Length == 0)
            return;

        System.Random prng = new System.Random(coord.x * 727 + coord.y * 947);

        int count = Mathf.RoundToInt(chunkSize * chunkSize * biome.environmentDensity);

        for (int i = 0; i < count; i++)
        {
            // выбираем окружение по весам
            EnvironmentEntry selected = SelectWeightedEnvironment(prng);
            if (selected == null) continue;

            float px = coord.x * chunkSize + (float)prng.NextDouble() * chunkSize;
            float pz = coord.y * chunkSize + (float)prng.NextDouble() * chunkSize;

            float h = BiomeHeightUtility.GetHeight(biome, px, pz);
            Vector3 pos = new Vector3(px, h, pz);

            Object.Instantiate(selected.prefab, pos, Quaternion.identity, parent);
        }
    }

    private EnvironmentEntry SelectWeightedEnvironment(System.Random rng)
    {
        float total = 0;
        foreach (var e in biome.environmentPrefabs)
            total += e.weight;

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
