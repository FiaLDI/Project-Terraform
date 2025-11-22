using UnityEngine;
using System.Collections.Generic;

public class Chunk
{
    public List<Vector3> environmentBlockers = new List<Vector3>();

    public Vector2Int coord;
    public bool IsLoaded => rootObject != null;

    private GameObject rootObject;
    private readonly WorldConfig world;

    private readonly int chunkSize;
    private readonly Transform parent;
    

    public Chunk(Vector2Int coord, WorldConfig world)
        : this(coord, world, world.chunkSize, null)
    { }

    public Chunk(Vector2Int coord, WorldConfig world, int chunkSize, Transform parent = null)
    {
        this.coord = coord;
        this.world = world;
        this.chunkSize = chunkSize;
        this.parent = parent;
    }

    // ============================
    // RUNTIME LOAD
    // ============================
    public void Load()
    {
        if (IsLoaded) return;

        rootObject = new GameObject($"Chunk_{coord.x}_{coord.y}");
        if (parent != null)
            rootObject.transform.SetParent(parent);

        GenerateLOD();
        SpawnEnvironment();
        SpawnResources();
        SpawnQuests();

        // ❌ НЕТ SpawnWater — вода теперь глобальная
    }

    public void LoadImmediate()
    {
        if (IsLoaded) return;

        rootObject = new GameObject($"Chunk_{coord.x}_{coord.y}");
        if (parent != null)
            rootObject.transform.SetParent(parent);

        GenerateImmediateMesh();
        SpawnEnvironment();
        SpawnResources();
        SpawnQuests();
    }

    // ============================
    // TERRAIN GENERATION
    // ============================

    private void GenerateLOD()
    {
        var blend = world.GetDominantBiome(coord);
        BiomeConfig biome = blend.biome;

        if (biome == null)
        {
            Debug.LogWarning($"Chunk {coord}: no biome found.");
            return;
        }

        var lod = new TerrainLOD(
            coord,
            biome,
            world,
            chunkSize,
            rootObject.transform
        );

        lod.Generate();
    }

    private void GenerateImmediateMesh()
    {
        Mesh m = TerrainMeshGenerator.GenerateMeshSync(
            coord,
            chunkSize,
            chunkSize,
            world
        );

        var go = new GameObject("Mesh");
        go.transform.SetParent(rootObject.transform);

        var mf = go.AddComponent<MeshFilter>();
        var mr = go.AddComponent<MeshRenderer>();

        var biome = world.GetBiomeAtChunk(coord);

        mr.sharedMaterial = biome != null ? biome.groundMaterial : null;
        mf.sharedMesh = m;
    }

    // ============================
    // SPAWNER
    // ============================

    private void SpawnEnvironment()
    {
        var biome = world.GetBiomeAtChunk(coord);
        if (biome == null) return;

        new EnvironmentChunkSpawner(coord, chunkSize, biome, rootObject.transform, environmentBlockers).Spawn();
    }

    private void SpawnResources()
    {
        var biome = world.GetBiomeAtChunk(coord);
        if (biome == null) return;

        new WorldResourceSpawner(coord, chunkSize, biome, rootObject.transform, environmentBlockers).Spawn();
    }

    private void SpawnQuests()
    {
        var biome = world.GetBiomeAtChunk(coord);
        if (biome == null) return;

        new QuestChunkSpawner(coord, chunkSize, biome, rootObject.transform).Spawn();
    }

    // ============================
    // UNLOAD
    // ============================

    public void Unload(int unloadDist, Vector2Int playerChunk)
    {
        if (!IsLoaded) return;

        int dx = Mathf.Abs(coord.x - playerChunk.x);
        int dy = Mathf.Abs(coord.y - playerChunk.y);

        if (dx > unloadDist || dy > unloadDist)
        {
            Object.Destroy(rootObject);
            rootObject = null;
        }
    }
}
