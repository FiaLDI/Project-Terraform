using UnityEngine;

[CreateAssetMenu(menuName = "Game/World Config")]
public class WorldConfig : ScriptableObject
{
    [Header("Размер мира в чанках")]
    public int worldWidthChunks = 32;
    public int worldHeightChunks = 32;
    public int chunkSize = 32;

    [Header("Шум биомов")]
    [Tooltip("Чем меньше значение – тем крупнее регионы биомов")]
    public float biomeNoiseScale = 0.02f;

    [Header("Слои биомов")]
    public BiomeLayer[] biomes;

    /// <summary>
    /// Выбор доминирующего биома в точке.
    /// </summary>
    public BiomeConfig GetBiomeAtWorldPos(Vector3 worldPos)
    {
        if (biomes == null || biomes.Length == 0)
            return null;

        float bestScore = float.MinValue;
        BiomeConfig best = biomes[0].config;

        foreach (var layer in biomes)
        {
            if (layer.config == null || layer.weight <= 0) continue;

            float n = Mathf.PerlinNoise(
                worldPos.x * biomeNoiseScale + layer.noiseOffset.x,
                worldPos.z * biomeNoiseScale + layer.noiseOffset.y
            );

            float score = n * layer.weight;
            if (score > bestScore)
            {
                bestScore = score;
                best = layer.config;
            }
        }

        return best;
    }

    /// <summary>
    /// Плавный микс нескольких биомов.
    /// </summary>
    public BiomeBlendResult[] GetBiomeBlend(Vector3 worldPos)
    {
        if (biomes == null || biomes.Length == 0)
            return null;

        int len = biomes.Length;
        BiomeBlendResult[] result = new BiomeBlendResult[len];

        float[] scores = new float[len];
        float totalScore = 0f;

        for (int i = 0; i < len; i++)
        {
            var layer = biomes[i];
            if (layer.config == null || layer.weight <= 0f)
            {
                scores[i] = 0f;
                continue;
            }

            float n = Mathf.PerlinNoise(
                worldPos.x * biomeNoiseScale + layer.noiseOffset.x,
                worldPos.z * biomeNoiseScale + layer.noiseOffset.y
            );

            float score = Mathf.Max(0f, n * layer.weight);
            scores[i] = score;
            totalScore += score;
        }

        if (totalScore <= 0f)
        {
            for (int i = 0; i < len; i++)
            {
                result[i].biome = biomes[i].config;
                result[i].weight = (i == 0 ? 1f : 0f);
            }
        }
        else
        {
            for (int i = 0; i < len; i++)
            {
                result[i].biome = biomes[i].config;
                result[i].weight = scores[i] / totalScore;
            }
        }

        return result;
    }

    public BiomeConfig GetBiomeAtChunk(Vector2Int chunk)
    {
        Vector3 worldPos = new Vector3(
            chunk.x * chunkSize,
            0f,
            chunk.y * chunkSize
        );

        return GetBiomeAtWorldPos(worldPos);
    }

    public BiomeBlendResult GetDominantBiome(Vector2Int chunk)
    {
        Vector3 pos = new Vector3(chunk.x * chunkSize, 0, chunk.y * chunkSize);

        var blends = GetBiomeBlend(pos);
        if (blends == null || blends.Length == 0) 
            return default;

        BiomeBlendResult best = blends[0];
        for (int i = 1; i < blends.Length; i++)
        {
            if (blends[i].weight > best.weight)
                best = blends[i];
        }
        return best;
    }

}

[System.Serializable]
public class BiomeLayer
{
    public BiomeConfig config;
    public Color debugColor = Color.white;
    public float weight = 1f;
    public Vector2 noiseOffset;
}

public struct BiomeBlendResult
{
    public BiomeConfig biome;
    public float weight;
}
