using UnityEngine;

public class EnvironmentSpawner
{
    private readonly Vector2Int coord;
    private readonly int chunkSize;
    private readonly BiomeConfig biome;
    private readonly Transform parent;

    public EnvironmentSpawner(Vector2Int coord, int chunkSize, BiomeConfig biome, Transform parent)
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

        int totalCount = Mathf.RoundToInt(chunkSize * chunkSize * biome.environmentDensity);
        if (totalCount <= 0) return;

        System.Random prng = new System.Random(coord.x * 73856093 ^ coord.y * 19349663);

        for (int i = 0; i < totalCount; i++)
        {
            EnvironmentEntry entry = GetWeightedRandomEntry(biome.environmentPrefabs, prng);
            if (entry == null || entry.prefab == null) continue;

            if (prng.NextDouble() > entry.spawnChance)
                continue;

            float px = coord.x * chunkSize + (float)prng.NextDouble() * chunkSize;
            float pz = coord.y * chunkSize + (float)prng.NextDouble() * chunkSize;

            float h = BiomeHeightUtility.GetHeight(biome, px, pz);

            Vector3 pos = new Vector3(px, h, pz);
            Quaternion rot = Quaternion.Euler(0f, (float)prng.NextDouble() * 360f, 0f);
            float scale = Mathf.Lerp(0.9f, 1.1f, (float)prng.NextDouble());

            GameObject obj = Object.Instantiate(entry.prefab, pos, rot, parent);
            obj.transform.localScale *= scale;
        }
    }

    private EnvironmentEntry GetWeightedRandomEntry(EnvironmentEntry[] entries, System.Random prng)
    {
        float total = 0f;
        foreach (var e in entries)
            total += Mathf.Max(0.01f, e.weight);

        float r = (float)prng.NextDouble() * total;
        float sum = 0f;

        foreach (var e in entries)
        {
            sum += Mathf.Max(0.01f, e.weight);
            if (r <= sum)
                return e;
        }

        return entries.Length > 0 ? entries[0] : null;
    }
}
