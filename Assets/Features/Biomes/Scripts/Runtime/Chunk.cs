using UnityEngine;

public class Chunk
{
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
    //   RUNTIME LOAD
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
        SpawnWater();
    }

    // ============================
    //   EDITOR / SYNC LOAD
    // ============================
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
        SpawnWater();
    }

    // ============================
    //   TERRAIN GENERATION
    // ============================

    /// <summary>
    /// Runtime LOD terrain (TerrainLOD + LODGroup).
    /// </summary>
    private void GenerateLOD()
    {
        // доминирующий биом для этого чанка
        var blend = world.GetDominantBiome(coord);
        BiomeConfig biome = blend.biome;

        if (biome == null)
        {
            Debug.LogWarning($"Chunk {coord}: no biome found for LOD generation.");
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

    /// <summary>
    /// Синхронная генерация одного меша (для редактора / префаба).
    /// </summary>
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
    //  SPAWN SYSTEMS
    // ============================

    private void SpawnEnvironment()
    {
        var biome = world.GetBiomeAtChunk(coord);
        if (biome == null) return;

        var spawner = new EnvironmentChunkSpawner(
            coord,
            chunkSize,
            biome,
            rootObject.transform
        );

        spawner.Spawn();
    }

    private void SpawnResources()
    {
        var biome = world.GetBiomeAtChunk(coord);
        if (biome == null) return;

        var spawner = new WorldResourceSpawner(
            coord,
            chunkSize,
            biome,
            rootObject.transform
        );

        spawner.Spawn();
    }

    private void SpawnQuests()
    {
        var biome = world.GetBiomeAtChunk(coord);
        if (biome == null) return;

        var spawner = new QuestChunkSpawner(
            coord,
            chunkSize,
            biome,
            rootObject.transform
        );

        spawner.Spawn();
    }

    private void SpawnWater()
    {
        var biome = world.GetBiomeAtChunk(coord);
        if (biome == null) return;

        var waterSpawner = new WaterChunkSpawner(
            coord,
            chunkSize,
            biome,
            rootObject.transform
        );

        waterSpawner.Spawn();
    }

    // ============================
    //   UNLOAD
    // ============================
    public void Unload(int unloadDist, Vector2Int playerChunk)
    {
        if (!IsLoaded) return;

        int dx = Mathf.Abs(coord.x - playerChunk.x);
        int dy = Mathf.Abs(coord.y - playerChunk.y);

        if (dx > unloadDist || dy > unloadDist)
        {
            GameObject.Destroy(rootObject);
            rootObject = null;
        }
    }
}
