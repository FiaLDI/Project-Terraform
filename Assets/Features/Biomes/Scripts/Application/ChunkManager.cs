using UnityEngine;
using System.Collections.Generic;
using Features.Biomes.Application;
using Features.Biomes.Application.Spawning;
using Features.Biomes.Domain;

public class ChunkManager
{
    private readonly WorldConfig world;
    private readonly int chunkSize;

    private readonly Dictionary<Vector2Int, Chunk> chunks = new();

    // Активные чанк-координаты
    private readonly List<Vector2Int> activeChunkCoords = new();
    private readonly List<ChunkRuntimeData> activeRuntimeChunks = new();

    // Очередь загрузки чанков
    private readonly Queue<Vector2Int> loadQueue = new();

    public int chunksPerFrame = 2;

    public ChunkManager(WorldConfig worldConfig)
    {
        world = worldConfig;
        chunkSize = worldConfig.chunkSize;

        ChunkedGameObjectStorage.chunkSize = chunkSize;
    }

    // ========================================================
    // ПОИСК НУЖНЫХ ЧАНКОВ
    // ========================================================
    public void UpdateChunks(Vector3 playerPos, int loadDist, int unloadDist)
    {
        Vector2Int playerChunk = new Vector2Int(
            Mathf.FloorToInt(playerPos.x / chunkSize),
            Mathf.FloorToInt(playerPos.z / chunkSize)
        );

        HashSet<Vector2Int> needed = new();

        // вычисляем зону загрузки
        for (int dz = -loadDist; dz <= loadDist; dz++)
        {
            for (int dx = -loadDist; dx <= loadDist; dx++)
            {
                Vector2Int coord = playerChunk + new Vector2Int(dx, dz);
                needed.Add(coord);

                if (!chunks.ContainsKey(coord))
                {
                    chunks[coord] = new Chunk(coord, world, chunkSize);
                    loadQueue.Enqueue(coord);
                }
                else if (!chunks[coord].IsLoaded)
                {
                    loadQueue.Enqueue(coord);
                }
            }
        }

        // Выгружаем те, что далеко
        foreach (var kv in chunks)
        {
            if (!needed.Contains(kv.Key))
                kv.Value.Unload(unloadDist, playerChunk);
        }

        // Активные координаты
        activeChunkCoords.Clear();
        activeChunkCoords.AddRange(needed);

        // runtimeChunks (для LOD system)
        activeRuntimeChunks.Clear();
        activeRuntimeChunks.AddRange(
            ChunkedGameObjectStorage.GetActiveChunks(activeChunkCoords)
        );

        if (ChunkedInstanceLODSystem.Instance != null)
        {
            ChunkedInstanceLODSystem.Instance.UpdateVisibleChunks(activeRuntimeChunks);
        }
    }

    // ========================================================
    // ОБРАБОТКА ОЧЕРЕДИ ЗАГРУЗКИ
    // ========================================================
    public void ProcessLoadQueue()
    {
        int count = Mathf.Min(chunksPerFrame, loadQueue.Count);

        for (int i = 0; i < count; i++)
        {
            Vector2Int coord = loadQueue.Dequeue();

            if (chunks.TryGetValue(coord, out var chunk) && !chunk.IsLoaded)
            {
                chunk.Load();
            }
        }
    }
}
