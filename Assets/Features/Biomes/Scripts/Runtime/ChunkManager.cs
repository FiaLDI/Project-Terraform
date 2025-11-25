using UnityEngine;
using System.Collections.Generic;
using System.Linq;


public class ChunkManager
{
    private readonly WorldConfig world;
    private readonly int chunkSize;

    private readonly Dictionary<Vector2Int, Chunk> chunks = new();

    public ChunkManager(WorldConfig worldConfig)
    {
        this.world = worldConfig;
        this.chunkSize = worldConfig.chunkSize;
    }

    public void UpdateChunks(Vector3 playerPos, int loadDist, int unloadDist)
    {
        Vector2Int playerChunk = new Vector2Int(
            Mathf.FloorToInt(playerPos.x / chunkSize),
            Mathf.FloorToInt(playerPos.z / chunkSize)
        );

        HashSet<Vector2Int> needed = new();

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

        foreach (var kv in chunks)
        {
            if (!needed.Contains(kv.Key))
                kv.Value.Unload(unloadDist, playerChunk);
        }
    }

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
