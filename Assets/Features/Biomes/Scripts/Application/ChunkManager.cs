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
    private readonly List<Vector2Int> activeChunkCoords = new();

    private readonly List<ChunkRuntimeData> activeRuntimeChunks = new();

    private readonly Queue<Vector2Int> loadQueue = new();
    private readonly HashSet<Vector2Int> neededSet = new();
    private readonly List<Vector2Int> chunksToRemove = new();

    public int chunksPerFrame = 2;

    private Vector2Int _lastPlayerChunk;
    private bool _hasLastPlayerChunk = false;

    public ChunkManager(WorldConfig worldConfig)
    {
        world = worldConfig;
        chunkSize = worldConfig.chunkSize;

        ChunkedGameObjectStorage.chunkSize = chunkSize;
    }

    // ========================================================
    // MAIN UPDATE
    // ========================================================
    public void UpdateChunks(Vector3 playerPos, int loadDist, int unloadDist)
    {
        Vector2Int playerChunk = new Vector2Int(
            Mathf.FloorToInt(playerPos.x / chunkSize),
            Mathf.FloorToInt(playerPos.z / chunkSize)
        );

        bool playerChunkChanged = !_hasLastPlayerChunk || playerChunk != _lastPlayerChunk;
        _lastPlayerChunk = playerChunk;
        _hasLastPlayerChunk = true;

        if (playerChunkChanged)
        {
            RebuildChunksAroundPlayer(playerChunk, loadDist, unloadDist);
        }

        // Build runtime data for LOD system
        ChunkedGameObjectStorage.FillActiveChunks(activeChunkCoords, activeRuntimeChunks);

        if (ChunkedInstanceLODSystem.Instance != null)
            ChunkedInstanceLODSystem.Instance.UpdateVisibleChunks(activeRuntimeChunks);

        // === DEBUG METRICS ===
        WorldDebugRegistry.ActiveChunkCount = activeChunkCoords.Count;
        WorldDebugRegistry.LoadedChunkCount = chunks.Count;
        WorldDebugRegistry.LoadQueueLength = loadQueue.Count;
    }

    // ========================================================
    // BUILD REQUIRED CHUNKS AROUND PLAYER
    // ========================================================
    private void RebuildChunksAroundPlayer(Vector2Int playerChunk, int loadDist, int unloadDist)
    {
        neededSet.Clear();

        // 1) Compute required chunks
        for (int dz = -loadDist; dz <= loadDist; dz++)
        {
            for (int dx = -loadDist; dx <= loadDist; dx++)
            {
                Vector2Int coord = playerChunk + new Vector2Int(dx, dz);
                neededSet.Add(coord);

                if (!chunks.TryGetValue(coord, out var chunk))
                {
                    chunk = new Chunk(coord, world, chunkSize);
                    chunks[coord] = chunk;
                    loadQueue.Enqueue(coord);
                }
                else if (!chunk.IsLoaded)
                {
                    loadQueue.Enqueue(coord);
                }
            }
        }

        // 2) Unload removed chunks
        chunksToRemove.Clear();

        foreach (var kv in chunks)
        {
            Vector2Int coord = kv.Key;
            Chunk chunk = kv.Value;

            if (!neededSet.Contains(coord))
            {
                chunk.Unload(unloadDist, playerChunk);
                chunksToRemove.Add(coord);
            }
        }

        for (int i = 0; i < chunksToRemove.Count; i++)
            chunks.Remove(chunksToRemove[i]);

        // 3) Build active list
        activeChunkCoords.Clear();
        activeChunkCoords.AddRange(neededSet);
    }

    // ========================================================
    // PROCESS LOAD QUEUE
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

    // ========================================================
    // FULL CLEAR
    // ========================================================
    public void ClearAll()
    {
        foreach (var kv in chunks)
            kv.Value.Unload(int.MaxValue, _lastPlayerChunk);

        chunks.Clear();
        loadQueue.Clear();
        activeChunkCoords.Clear();
        activeRuntimeChunks.Clear();
        neededSet.Clear();
        chunksToRemove.Clear();

        _hasLastPlayerChunk = false;

        // Debug reset
        WorldDebugRegistry.ActiveChunkCount = 0;
        WorldDebugRegistry.LoadedChunkCount = 0;
        WorldDebugRegistry.LoadQueueLength = 0;
    }
}
