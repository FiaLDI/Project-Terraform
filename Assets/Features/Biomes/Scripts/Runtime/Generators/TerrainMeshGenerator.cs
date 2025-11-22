using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public static class TerrainMeshGenerator
{
    private static readonly Dictionary<string, Mesh> meshCache =
        new Dictionary<string, Mesh>();

    private static readonly Queue<Action> unityThreadQueue =
        new Queue<Action>();

    public static void ExecutePendingUnityThreadTasks()
    {
        while (unityThreadQueue.Count > 0)
            unityThreadQueue.Dequeue()?.Invoke();
    }

    // ============================================================
    // SYNC
    // ============================================================
    public static Mesh GenerateMeshSync(
        Vector2Int coord,
        int chunkSize,
        int resolution,
        WorldConfig worldConfig)
    {
        var data = GenerateMeshData(coord, chunkSize, resolution, worldConfig);

        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = data.vertices;
        mesh.triangles = data.triangles;
        mesh.RecalculateNormals();

        return mesh;
    }

    // ============================================================
    // ASYNC
    // ============================================================
    public static async Task<Mesh> GenerateMeshAsync(
        Vector2Int coord,
        int chunkSize,
        int resolution,
        WorldConfig worldConfig)
    {
        string key = GetCacheKey(coord, resolution, worldConfig);

        if (meshCache.TryGetValue(key, out Mesh cached))
            return cached;

        var data = await Task.Run(() =>
            GenerateMeshData(coord, chunkSize, resolution, worldConfig)
        );

        var tcs = new TaskCompletionSource<Mesh>();

        unityThreadQueue.Enqueue(() =>
        {
            Mesh mesh = new Mesh();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.vertices = data.vertices;
            mesh.triangles = data.triangles;
            mesh.RecalculateNormals();

            meshCache[key] = mesh;
            tcs.SetResult(mesh);
        });

        return await tcs.Task;
    }

    // ============================================================
    // CORE TERRAIN GENERATION
    // ============================================================
    private static (Vector3[] vertices, int[] triangles) GenerateMeshData(
        Vector2Int coord,
        int chunkSize,
        int resolution,
        WorldConfig worldConfig)
    {
        int vertsPerLine = resolution + 1;
        Vector3[] vertices = new Vector3[vertsPerLine * vertsPerLine];
        int[] triangles = new int[resolution * resolution * 6];

        float step = (float)chunkSize / resolution;

        for (int z = 0, i = 0; z <= resolution; z++)
        {
            for (int x = 0; x <= resolution; x++, i++)
            {
                float wx = coord.x * chunkSize + x * step;
                float wz = coord.y * chunkSize + z * step;

                Vector3 worldPos = new Vector3(wx, 0, wz);

                // Получаем список биомов и весов
                var blend = worldConfig.GetBiomeBlend(worldPos);

                float height = 0f;

                float nearestOcean = float.MaxValue;
                float nearestLake = float.MaxValue;
                float nearestSwamp = float.MaxValue;

                // ============================================================
                // 1) BIOME HEIGHT BLENDING
                // ============================================================
                foreach (var bi in blend)
                {
                    if (bi.biome == null || bi.weight <= 0f) continue;

                    // базовый рельеф
                    float h = BiomeHeightUtility.GetHeight(bi.biome, wx, wz);
                    height += h * bi.weight;

                    // классификация воды
                    if (bi.biome.useWater)
                    {
                        switch (bi.biome.waterType)
                        {
                            case WaterType.Ocean:
                                nearestOcean = Mathf.Min(nearestOcean, bi.biome.seaLevel);
                                break;

                            case WaterType.Lake:
                                nearestLake = Mathf.Min(nearestLake, bi.biome.seaLevel);
                                break;

                            case WaterType.Swamp:
                                nearestSwamp = Mathf.Min(nearestSwamp, bi.biome.seaLevel);
                                break;
                        }
                    }
                }

                // ============================================================
                // 2) RIVERS
                // ============================================================
                foreach (var bi in blend)
                {
                    var b = bi.biome;
                    float w = bi.weight;

                    if (b == null || !b.generateRivers || w <= 0f)
                        continue;

                    float rn = Mathf.PerlinNoise(
                        wx * b.riverNoiseScale,
                        wz * b.riverNoiseScale
                    );

                    float center = Mathf.Abs(rn - 0.5f);
                    float mask = Mathf.Clamp01((0.5f - center) * (1f / b.riverWidth));

                    height -= mask * b.riverDepth * w;
                }

                // ============================================================
                // 3) LAKES (локальные, НЕ глобальная вода)
                // ============================================================
                foreach (var bi in blend)
                {
                    var b = bi.biome;
                    float w = bi.weight;

                    if (b == null || !b.generateLakes || w <= 0f) continue;

                    float ln = Mathf.PerlinNoise(
                        wx * b.lakeNoiseScale,
                        wz * b.lakeNoiseScale
                    );

                    if (ln > b.lakeLevel)
                    {
                        float t = (ln - b.lakeLevel) / (1f - b.lakeLevel);

                        float lakeBottom = b.seaLevel - 0.8f;

                        height = Mathf.Lerp(height, lakeBottom, t * w);
                    }
                }

                // ============================================================
                // 4) SHORELINES FOR EACH WATER TYPE
                // ============================================================

                // Ocean shores — smooth & soft
                if (nearestOcean < float.MaxValue)
                {
                    float dh = height - nearestOcean;

                    if (dh < 4f)
                    {
                        float t = Mathf.Clamp01(dh / 4f);
                        height = Mathf.Lerp(nearestOcean - 2f, height, t);
                    }
                }

                // Lake shores — sharper
                if (nearestLake < float.MaxValue)
                {
                    float dh = height - nearestLake;

                    if (dh < 2f)
                    {
                        float t = Mathf.Clamp01(dh / 2f);
                        height = Mathf.Lerp(nearestLake - 1f, height, t);
                    }
                }

                // Swamp — almost no slope, very soft
                if (nearestSwamp < float.MaxValue)
                {
                    float dh = height - nearestSwamp;

                    if (dh < 1.5f)
                    {
                        float t = Mathf.Clamp01(dh / 1.5f);
                        height = Mathf.Lerp(nearestSwamp - 0.5f, height, t * 0.7f);
                    }
                }

                // ============================================================
                // 5) UNDERWATER SMOOTHING
                // ============================================================
                float nearestWater = Mathf.Min(nearestOcean, Mathf.Min(nearestLake, nearestSwamp));

                if (nearestWater < float.MaxValue && height < nearestWater)
                {
                    float depth = nearestWater - height;
                    float smooth = Mathf.SmoothStep(0, 1, depth * 0.15f);
                    height = Mathf.Lerp(height, height - depth * 0.35f, smooth);
                }

                vertices[i] = new Vector3(wx, height, wz);
            }
        }

        // ============================================================
        // TRIANGLES
        // ============================================================
        for (int z = 0, v = 0, t = 0; z < resolution; z++, v++)
        {
            for (int x = 0; x < resolution; x++, v++, t += 6)
            {
                triangles[t + 0] = v;
                triangles[t + 1] = v + vertsPerLine;
                triangles[t + 2] = v + 1;

                triangles[t + 3] = v + 1;
                triangles[t + 4] = v + vertsPerLine;
                triangles[t + 5] = v + vertsPerLine + 1;
            }
        }

        return (vertices, triangles);
    }

    private static string GetCacheKey(Vector2Int coord, int resolution, WorldConfig worldConfig)
    {
        return worldConfig.GetInstanceID() + "_" + coord.x + "_" + coord.y + "_" + resolution;
    }
}
