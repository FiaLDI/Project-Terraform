using UnityEngine;
using System.Collections.Generic;
using Biomes.Data;
using Biomes.UnityIntegration;

namespace Biomes.Application { 
    public class ChunkManager
    {
        private readonly WorldConfig world;
        private readonly int chunkSize;

        private readonly Dictionary<Vector2Int, Chunk> chunks = new();
        private readonly List<Vector2Int> activeChunkCoords = new();

        private readonly List<ChunkRuntimeData> activeRuntimeChunks = new();

        private readonly Queue<Vector2Int> loadQueue = new();
        private readonly HashSet<Vector2Int> queuedChunks = new();
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

            ChunkedGameObjectStorage.FillActiveChunks(activeChunkCoords, activeRuntimeChunks);

            if (ChunkedInstanceLODSystem.Instance != null)
                ChunkedInstanceLODSystem.Instance.UpdateVisibleChunks(activeRuntimeChunks);
        }

        public void UpdateChunks(IReadOnlyList<Vector3> playerPositions, int loadDist, int unloadDist)
        {
            if (playerPositions == null || playerPositions.Count == 0)
                return;

            RebuildChunksAroundPlayers(playerPositions, loadDist, unloadDist);

            ChunkedGameObjectStorage.FillActiveChunks(activeChunkCoords, activeRuntimeChunks);

            if (ChunkedInstanceLODSystem.Instance != null)
                ChunkedInstanceLODSystem.Instance.UpdateVisibleChunks(activeRuntimeChunks);
        }

        // ========================================================
        // BUILD REQUIRED CHUNKS AROUND PLAYER
        // ========================================================
        private void RebuildChunksAroundPlayer(Vector2Int playerChunk, int loadDist, int unloadDist)
        {
            neededSet.Clear();

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
                        ChunkedGameObjectStorage.EnsureChunk(coord);
                        EnqueueChunk(coord);
                    }
                    else if (!chunk.IsLoaded)
                    {
                        ChunkedGameObjectStorage.EnsureChunk(coord);
                        EnqueueChunk(coord);
                    }
                }
            }

            chunksToRemove.Clear();

            foreach (var kv in chunks)
            {
                Vector2Int coord = kv.Key;
                Chunk chunk = kv.Value;

                if (!neededSet.Contains(coord))
                {
                    chunk.Unload(unloadDist, playerChunk);
                    chunksToRemove.Add(coord);
                    queuedChunks.Remove(coord);
                }
            }

            for (int i = 0; i < chunksToRemove.Count; i++)
                chunks.Remove(chunksToRemove[i]);

            activeChunkCoords.Clear();
            activeChunkCoords.AddRange(neededSet);
        }

        private void RebuildChunksAroundPlayers(IReadOnlyList<Vector3> playerPositions, int loadDist, int unloadDist)
        {
            neededSet.Clear();

            for (int p = 0; p < playerPositions.Count; p++)
            {
                Vector3 playerPos = playerPositions[p];
                Vector2Int playerChunk = new Vector2Int(
                    Mathf.FloorToInt(playerPos.x / chunkSize),
                    Mathf.FloorToInt(playerPos.z / chunkSize)
                );

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
                            ChunkedGameObjectStorage.EnsureChunk(coord);
                            EnqueueChunk(coord);
                        }
                        else if (!chunk.IsLoaded)
                        {
                            ChunkedGameObjectStorage.EnsureChunk(coord);
                            EnqueueChunk(coord);
                        }
                    }
                }
            }

            chunksToRemove.Clear();

            Vector2Int fallbackPlayerChunk = playerPositions.Count > 0
                ? new Vector2Int(
                    Mathf.FloorToInt(playerPositions[0].x / chunkSize),
                    Mathf.FloorToInt(playerPositions[0].z / chunkSize)
                )
                : default;

            foreach (var kv in chunks)
            {
                Vector2Int coord = kv.Key;
                Chunk chunk = kv.Value;

                if (!neededSet.Contains(coord))
                {
                    chunk.Unload(unloadDist, fallbackPlayerChunk);
                    chunksToRemove.Add(coord);
                    queuedChunks.Remove(coord);
                }
            }

            for (int i = 0; i < chunksToRemove.Count; i++)
                chunks.Remove(chunksToRemove[i]);

            activeChunkCoords.Clear();
            activeChunkCoords.AddRange(neededSet);
        }

        private void EnqueueChunk(Vector2Int coord)
        {
            if (!queuedChunks.Add(coord))
                return;

            loadQueue.Enqueue(coord);
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
                queuedChunks.Remove(coord);

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
            queuedChunks.Clear();
            activeChunkCoords.Clear();
            activeRuntimeChunks.Clear();
            neededSet.Clear();
            chunksToRemove.Clear();

            _hasLastPlayerChunk = false;
        }
    }
}
