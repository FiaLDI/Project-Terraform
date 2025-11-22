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

    public static Mesh GenerateMeshSync(
    Vector2Int coord,
    int chunkSize,
    int resolution,
    WorldConfig worldConfig,
    bool lowPoly)
    {
        var data = GenerateMeshData(coord, chunkSize, resolution, worldConfig);

        Mesh mesh; // ✅ ВОТ ЭТОГО НЕ ХВАТАЛО

        if (lowPoly)
        {
            mesh = ConvertToLowPoly(data.vertices, data.triangles);
        }
        else
        {
            mesh = new Mesh();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.vertices = data.vertices;
            mesh.triangles = data.triangles;
            mesh.RecalculateNormals();
        }

        return mesh;
    }

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
    //                FINAL FULL GENERATION FUNCTION
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

                var blend = worldConfig.GetBiomeBlend(worldPos);

                float height = 0f;
                float nearestSea = float.MaxValue;

                // ============================================================
                //                1) BIOME HEIGHT BLENDING
                // ============================================================
                foreach (var bi in blend)
                {
                    if (bi.biome == null || bi.weight <= 0f) continue;

                    float h = BiomeHeightUtility.GetHeight(bi.biome, wx, wz);
                    height += h * bi.weight;

                    // nearest water among biomes
                    if (bi.biome.useWater)
                        nearestSea = Mathf.Min(nearestSea, bi.biome.seaLevel);
                }

                // ============================================================
                //                2) RIVERS
                // ============================================================
                foreach (var bi in blend)
                {
                    var b = bi.biome;
                    float w = bi.weight;

                    if (b == null || !b.generateRivers || w <= 0f) continue;

                    float rn = Mathf.PerlinNoise(
                        wx * b.riverNoiseScale,
                        wz * b.riverNoiseScale
                    );

                    float center = Mathf.Abs(rn - 0.5f);
                    float mask = Mathf.Clamp01((0.5f - center) * (1f / b.riverWidth));

                    height -= mask * b.riverDepth * w;
                }

                // ============================================================
                //                3) LAKES
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
                        float target = b.seaLevel - 1f;

                        height = Mathf.Lerp(height, target, t * w);
                    }
                }

                // ============================================================
                //                4) SHORE-LINE SMOOTH FUSION
                // ============================================================
                if (nearestSea < float.MaxValue)
                {
                    float dh = height - nearestSea;

                    if (dh < 3f)
                    {
                        float t = Mathf.Clamp01(dh / 3f);
                        height = Mathf.Lerp(nearestSea - 1f, height, t);
                    }
                }

                // ============================================================
                //                5) UNDERWATER SMOOTHING
                // ============================================================
                if (nearestSea < float.MaxValue && height < nearestSea)
                {
                    float depth = nearestSea - height;
                    float smooth = Mathf.SmoothStep(0, 1, depth * 0.15f);
                    height = Mathf.Lerp(height, height - depth * 0.4f, smooth);
                }

                vertices[i] = new Vector3(wx, height, wz);
            }
        }

        // ============================================================
        //                TRIANGLES
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

    private static Mesh ConvertToLowPoly(Vector3[] vertices, int[] triangles)
    {
        Vector3[] flatVerts = new Vector3[triangles.Length];
        int[] flatTris = new int[triangles.Length];

        for (int i = 0; i < triangles.Length; i++)
        {
            flatVerts[i] = vertices[triangles[i]];
            flatTris[i] = i;
        }

        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = flatVerts;
        mesh.triangles = flatTris;
        mesh.RecalculateNormals();

        return mesh;
    }


    private static string GetCacheKey(Vector2Int coord, int resolution, WorldConfig worldConfig)
    {
        return worldConfig.GetInstanceID() + "_" + coord.x + "_" + coord.y + "_" + resolution;
    }
}
