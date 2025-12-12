using UnityEngine;
using Unity.Mathematics;
using Features.Biomes.Domain;

public static class BiomeHeightUtility
{
    public static float GetHeight(BiomeConfig biome, float x, float z)
    {
        if (biome == null)
            return 0f;

        float baseNoise = SafePerlin(
            x * biome.terrainScale * 0.01f,
            z * biome.terrainScale * 0.01f
        );

        float y = 0f;

        switch (biome.terrainType)
        {
            case TerrainType.SmoothHills:
                y = baseNoise * biome.heightMultiplier;
                break;

            case TerrainType.SharpMountains:
                y = Mathf.Pow(baseNoise, 3f) * biome.heightMultiplier;
                break;

            case TerrainType.Plateaus:
                y = Mathf.Round(baseNoise * 3f) / 3f * biome.heightMultiplier;
                break;

            case TerrainType.Craters:
                y = (1f - Mathf.Abs(baseNoise * 2f - 1f)) * biome.heightMultiplier;
                break;

            case TerrainType.Dunes:
                {
                    float dune = SafePerlin(
                        x * biome.terrainScale * 0.05f,
                        0f
                    );
                    y = dune * biome.heightMultiplier * 0.5f;
                    break;
                }

            case TerrainType.Islands:
                {
                    float dist = Vector2.Distance(
                        new Vector2(x, z),
                        new Vector2(biome.width / 2f, biome.height / 2f)
                    );
                    float gradient = Mathf.Clamp01(1f - dist / (biome.width / 2f));
                    y = baseNoise * biome.heightMultiplier * gradient;
                    break;
                }

            case TerrainType.Canyons:
                {
                    float canyon = Mathf.Abs(SafePerlin(x * 0.05f, 0f) - 0.5f) * 2f;
                    y = baseNoise * biome.heightMultiplier * canyon;
                    break;
                }

            case TerrainType.FractalMountains:
                y = RidgedNoise(
                    x, z,
                    biome.terrainScale * 0.01f,
                    biome.fractalOctaves,
                    biome.fractalPersistence,
                    biome.fractalLacunarity
                ) * biome.heightMultiplier;
                break;
        }

        // защита от NaN
        if (float.IsNaN(y) || float.IsInfinity(y))
            y = 0f;

        return y;
    }

    private static float SafePerlin(float x, float z)
    {
        // ограничиваем входные координаты, чтобы Perlin не уходил в странные режимы
        x = math.fmod(x, 10000f);
        z = math.fmod(z, 10000f);

        if (x < 0f) x += 10000f;
        if (z < 0f) z += 10000f;

        float n = Mathf.PerlinNoise(x, z);
        if (float.IsNaN(n) || float.IsInfinity(n))
            return 0f;

        return n;
    }

    private static float RidgedNoise(
        float x, float z,
        float scale, int octaves, float persistence, float lacunarity)
    {
        float total = 0f;
        float amplitude = 1f;
        float frequency = 1f;
        float maxValue = 0f;

        for (int i = 0; i < octaves; i++)
        {
            float n = SafePerlin(
                x * scale * frequency,
                z * scale * frequency
            );
            n = 1f - Mathf.Abs(n * 2f - 1f);
            total += n * amplitude;

            maxValue += amplitude;
            amplitude *= persistence;
            frequency *= lacunarity;
        }

        if (maxValue <= 0f)
            return 0f;

        float v = total / maxValue;
        if (float.IsNaN(v) || float.IsInfinity(v))
            return 0f;

        return v;
    }
}
