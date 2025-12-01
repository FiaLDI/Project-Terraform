using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Features.Biomes.Domain;
using Features.Biomes.Application;
using Features.Biomes.Application.Spawning;

public class ChunkManager
{
    private readonly WorldConfig world;
    private readonly int chunkSize;

    private readonly Dictionary<Vector2Int, Chunk> chunks = new();

    // LOD списки
    private readonly List<Vector2Int> activeChunkCoords = new();
    private readonly List<ChunkRuntimeData> activeRuntimeChunks = new();

    public ChunkManager(WorldConfig worldConfig)
    {
        this.world = worldConfig;
        this.chunkSize = worldConfig.chunkSize;

        // Новая система хранения → обновлённая
        ChunkedGameObjectStorage.chunkSize = chunkSize;
    }

    // ========================================================
    // UPDATE CHUNKS (LOAD + UNLOAD + UPDATE LOD)
    // ========================================================
    public void UpdateChunks(Vector3 playerPos, int loadDist, int unloadDist)
    {
        Vector2Int playerChunk = new Vector2Int(
            Mathf.FloorToInt(playerPos.x / chunkSize),
            Mathf.FloorToInt(playerPos.z / chunkSize)
        );

        HashSet<Vector2Int> needed = new();

        // LOAD AREA
        for (int dz = -loadDist; dz <= loadDist; dz++)
        {
            for (int dx = -loadDist; dx <= loadDist; dx++)
            {
                Vector2Int coord = playerChunk + new Vector2Int(dx, dz);
                needed.Add(coord);

                if (!chunks.ContainsKey(coord))
                {
                    Chunk c = new Chunk(coord, world, chunkSize);
                    chunks.Add(coord, c);
                    c.Load();
                }
                else if (!chunks[coord].IsLoaded)
                {
                    chunks[coord].Load();
                }
            }
        }

        // UNLOAD AREA
        foreach (var kv in chunks)
        {
            if (!needed.Contains(kv.Key))
                kv.Value.Unload(unloadDist, playerChunk);
        }

        // ACTIVE CHUNKS LIST
        activeChunkCoords.Clear();
        activeChunkCoords.AddRange(needed);

        // ACTIVE RUNTIME DATA
        activeRuntimeChunks.Clear();
        if (InstancedSpawnerSystem.Instance == null)
        {
            return;
        }

        activeRuntimeChunks.AddRange(
            ChunkedGameObjectStorage.GetActiveChunks(activeChunkCoords)
        );

        // UPDATE LOD SYSTEM
        if (ChunkedInstanceLODSystem.Instance != null)
        {
            ChunkedInstanceLODSystem.Instance.UpdateVisibleChunks(activeRuntimeChunks);
        }
    }

    // ========================================================
    // STATIC AREA
    // ========================================================
    public GameObject GenerateStaticArea(Vector2Int centerChunk, int radius)
    {
        GameObject root = new GameObject("Biome_StaticArea");

        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                Vector2Int coord = centerChunk + new Vector2Int(x, y);

                Chunk chunk = new Chunk(coord, world, world.chunkSize, root.transform);
                chunk.LoadImmediate();
            }
        }

        return root;
    }

    // ========================================================
    // BLOCKERS
    // ========================================================
    public List<Vector3> GetAllEnvironmentBlockers()
    {
        List<Vector3> result = new List<Vector3>();

        foreach (var kv in chunks)
        {
            if (!kv.Value.IsLoaded)
                continue;

            result.AddRange(kv.Value.environmentBlockers.Select(b => b.position));
        }

        return result;
    }
}
