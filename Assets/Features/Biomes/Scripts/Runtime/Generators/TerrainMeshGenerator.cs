using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System.Runtime.CompilerServices;

public static class TerrainMeshGenerator
{
    // ============================================================
    // CACHE
    // ============================================================
    private static readonly Dictionary<string, Mesh> meshCache =
        new Dictionary<string, Mesh>();

    private static readonly Queue<Action> unityThreadQueue =
        new Queue<Action>();

    // Кэш треугольников (для каждого resolution)
    private static readonly Dictionary<int, int[]> triCache =
        new Dictionary<int, int[]>();

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
        WorldConfig worldConfig,
        bool lowPoly)
    {
        (Vector3[] vertices, int[] triangles) data =
            GenerateMeshData(coord, chunkSize, resolution, worldConfig);

        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        if (lowPoly)
        {
            ConvertToLowPoly(mesh, data.vertices, data.triangles);
        }
        else
        {
            mesh.vertices = data.vertices;
            mesh.triangles = data.triangles;
            mesh.RecalculateNormals();
        }

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
    private static (Vector3[] vertices, int[] triangles)
        GenerateMeshData(
            Vector2Int coord,
            int chunkSize,
            int resolution,
            WorldConfig worldConfig)
    {
        int vertsPerLine = resolution + 1;
        int vertCount = vertsPerLine * vertsPerLine;

        // ----------- вершины ----------------
        Vector3[] vertices = new Vector3[vertCount];

        // ----------- треугольники (кэш) ------
        if (!triCache.TryGetValue(resolution, out int[] triangles))
        {
            triangles = GenerateAlternatingTriangles(resolution);
            triCache[resolution] = triangles;
        }

        float step = (float)chunkSize / resolution;

        float baseX = coord.x * chunkSize;
        float baseZ = coord.y * chunkSize;

        int i = 0;

        // ============================================================
        // FAST VERTEX GENERATION (optimized)
        // ============================================================
        for (int z = 0; z <= resolution; z++)
        {
            float wz = baseZ + z * step;

            for (int x = 0; x <= resolution; x++, i++)
            {
                float wx = baseX + x * step;

                // -------------------------
                // BIOME BLEND HEIGHT
                // -------------------------
                var blend = worldConfig.GetBiomeBlend(new Vector3(wx, 0, wz));

                float height = 0f;

                float nearestOcean = float.MaxValue;
                float nearestLake = float.MaxValue;
                float nearestSwamp = float.MaxValue;

                // ----- 1) BLEND BASE HEIGHT -----
                foreach (var bi in blend)
                {
                    if (bi.biome == null || bi.weight <= 0f) continue;

                    height += FastHeight(bi.biome, wx, wz) * bi.weight;

                    if (bi.biome.useWater)
                    {
                        float sl = bi.biome.seaLevel;

                        switch (bi.biome.waterType)
                        {
                            case WaterType.Ocean:
                                if (sl < nearestOcean) nearestOcean = sl;
                                break;

                            case WaterType.Lake:
                                if (sl < nearestLake) nearestLake = sl;
                                break;

                            case WaterType.Swamp:
                                if (sl < nearestSwamp) nearestSwamp = sl;
                                break;
                        }
                    }
                }

                // ----- 2) RIVERS -----
                foreach (var bi in blend)
                {
                    var b = bi.biome;
                    float w = bi.weight;
                    if (b == null || !b.generateRivers || w <= 0f) continue;

                    float rn = FastPerlin(wx * b.riverNoiseScale, wz * b.riverNoiseScale);
                    float center = FastAbs(rn - 0.5f);
                    float mask = FastClamp01((0.5f - center) * (1f / b.riverWidth));

                    height -= mask * b.riverDepth * w;
                }

                // ----- 3) LAKES -----
                foreach (var bi in blend)
                {
                    var b = bi.biome;
                    float w = bi.weight;
                    if (b == null || !b.generateLakes || w <= 0f) continue;

                    float ln = FastPerlin(wx * b.lakeNoiseScale, wz * b.lakeNoiseScale);

                    if (ln > b.lakeLevel)
                    {
                        float t = (ln - b.lakeLevel) / (1f - b.lakeLevel);
                        float lakeBottom = b.seaLevel - 0.8f;
                        height = FastLerp(height, lakeBottom, t * w);
                    }
                }

                // ----- 4) SHORELINES -----
                if (nearestOcean < float.MaxValue)
                {
                    float dh = height - nearestOcean;
                    if (dh < 4f)
                    {
                        float t = FastClamp01(dh / 4f);
                        height = FastLerp(nearestOcean - 2f, height, t);
                    }
                }

                if (nearestLake < float.MaxValue)
                {
                    float dh = height - nearestLake;
                    if (dh < 2f)
                    {
                        float t = FastClamp01(dh / 2f);
                        height = FastLerp(nearestLake - 1f, height, t);
                    }
                }

                if (nearestSwamp < float.MaxValue)
                {
                    float dh = height - nearestSwamp;
                    if (dh < 1.5f)
                    {
                        float t = FastClamp01(dh / 1.5f);
                        height = FastLerp(nearestSwamp - 0.5f, height, t * 0.7f);
                    }
                }

                // ----- 5) UNDERWATER SMOOTHING -----
                float nearestWater = FastMin(nearestOcean, FastMin(nearestLake, nearestSwamp));

                if (nearestWater < float.MaxValue && height < nearestWater)
                {
                    float depth = nearestWater - height;
                    float smooth = FastSmoothStep(0, 1, depth * 0.15f);
                    height = FastLerp(height, height - depth * 0.35f, smooth);
                }

                vertices[i].x = wx;
                vertices[i].y = height;
                vertices[i].z = wz;
            }
        }

        return (vertices, triangles);
    }

    // ============================================================
    // ALTERNATING TRIANGLE GRID (DIAMOND PATTERN)
    // ============================================================
    private static int[] GenerateAlternatingTriangles(int resolution)
    {
        int verts = resolution + 1;
        int tris = resolution * resolution * 6;
        int[] t = new int[tris];

        int index = 0;

        for (int z = 0, v = 0; z < resolution; z++, v++)
        {
            bool flip = (z & 1) == 1;

            for (int x = 0; x < resolution; x++, v++)
            {
                int v00 = v;
                int v10 = v + 1;
                int v01 = v + verts;
                int v11 = v + verts + 1;

                if ((x & 1) == 1)
                    flip = !flip;

                if (!flip)
                {
                    t[index++] = v00;
                    t[index++] = v01;
                    t[index++] = v11;

                    t[index++] = v00;
                    t[index++] = v11;
                    t[index++] = v10;
                }
                else
                {
                    t[index++] = v00;
                    t[index++] = v01;
                    t[index++] = v10;

                    t[index++] = v10;
                    t[index++] = v01;
                    t[index++] = v11;
                }
            }
        }

        return t;
    }

    // ============================================================
    // LOW POLY CONVERSION
    // ============================================================
    private static void ConvertToLowPoly(Mesh mesh, Vector3[] vertices, int[] triangles)
    {
        Vector3[] flatVerts = new Vector3[triangles.Length];
        int[] flatTris = new int[triangles.Length];

        for (int i = 0; i < triangles.Length; i++)
        {
            flatVerts[i] = vertices[triangles[i]];
            flatTris[i] = i;
        }

        mesh.vertices = flatVerts;
        mesh.triangles = flatTris;
        mesh.RecalculateNormals();
    }

    // ============================================================
    // FAST MATH
    // ============================================================
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float FastHeight(BiomeConfig b, float x, float z)
        => BiomeHeightUtility.GetHeight(b, x, z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float FastPerlin(float x, float y)
        => Mathf.PerlinNoise(x, y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float FastAbs(float v)
        => v >= 0 ? v : -v;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float FastLerp(float a, float b, float t)
        => a + (b - a) * t;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float FastClamp01(float v)
        => v <= 0 ? 0 : (v >= 1 ? 1 : v);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float FastMin(float a, float b)
        => a < b ? a : b;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float FastSmoothStep(float a, float b, float t)
    {
        t = FastClamp01((t - a) / (b - a));
        return t * t * (3f - 2f * t);
    }

    private static string GetCacheKey(Vector2Int coord, int resolution, WorldConfig worldConfig)
    {
        return worldConfig.GetInstanceID() + "_" + coord.x + "_" + coord.y + "_" + resolution;
    }
}
