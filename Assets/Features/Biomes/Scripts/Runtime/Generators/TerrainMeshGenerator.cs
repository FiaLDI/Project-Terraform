using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public static class TerrainMeshGenerator
{
    // Кэш мешей (по ключу: worldConfig + coord + resolution)
    private static readonly Dictionary<string, Mesh> meshCache =
        new Dictionary<string, Mesh>();

    // Очередь действий, которые нужно выполнить в Unity-потоке
    private static readonly Queue<Action> unityThreadQueue =
        new Queue<Action>();

    /// <summary>
    /// Вызывать каждый кадр в Editor/Runtime, чтобы выполнить отложенные действия
    /// (создание/присвоение мешей и т.п.) в Unity-потоке.
    /// </summary>
    public static void ExecutePendingUnityThreadTasks()
    {
        while (unityThreadQueue.Count > 0)
        {
            var action = unityThreadQueue.Dequeue();
            action?.Invoke();
        }
    }

    public static Mesh GenerateMeshSync(Vector2Int coord, int chunkSize, int resolution, WorldConfig worldConfig)
    {
        var data = GenerateMeshData(coord, chunkSize, resolution, worldConfig);
        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = data.vertices;
        mesh.triangles = data.triangles;
        mesh.RecalculateNormals();
        return mesh;
    }


    /// <summary>
    /// Асинхронная генерация меша чанка с учётом blending биомов.
    /// </summary>
    public static async Task<Mesh> GenerateMeshAsync(
        Vector2Int coord,
        int chunkSize,
        int resolution,
        WorldConfig worldConfig)
    {
        string key = GetCacheKey(coord, resolution, worldConfig);

        // Если уже есть в кэше — просто вернуть
        if (meshCache.TryGetValue(key, out Mesh cached))
            return cached;

        // Тяжёлые вычисления высот и треугольников — в Task
        var data = await Task.Run(() =>
            GenerateMeshData(coord, chunkSize, resolution, worldConfig)
        );

        // Затем в Unity-потоке собираем сам Mesh
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

    /// <summary>
    /// Генерирует только массивы вершин и треугольников (можно выполнять в любом потоке).
    /// </summary>
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

        // ВЕРШИНЫ
        for (int z = 0, i = 0; z <= resolution; z++)
        {
            for (int x = 0; x <= resolution; x++, i++)
            {
                float worldX = coord.x * chunkSize + x * step;
                float worldZ = coord.y * chunkSize + z * step;
                Vector3 worldPos = new Vector3(worldX, 0f, worldZ);

                BiomeBlendResult[] blend = worldConfig.GetBiomeBlend(worldPos);

                float height = 0f;

                if (blend != null)
                {
                    for (int b = 0; b < blend.Length; b++)
                    {
                        var bi = blend[b];
                        if (bi.biome == null || bi.weight <= 0f) continue;

                        float h = BiomeHeightUtility.GetHeight(bi.biome, worldX, worldZ);
                        height += h * bi.weight;
                    }
                }

                vertices[i] = new Vector3(worldX, height, worldZ);
            }
        }

        // ТРЕУГОЛЬНИКИ
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
        return worldConfig.GetInstanceID().ToString() + "_" +
               coord.x + "_" + coord.y + "_" + resolution;
    }
}
