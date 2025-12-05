using UnityEngine;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using Features.Biomes.Domain;
using Features.Biomes.Application;
using Features.Biomes.UnityIntegration;
using Features.Biomes.Utility;
using Features.Biomes.Application.Spawning;
using Unity.Jobs;

public class Chunk
{
    public List<Blocker> environmentBlockers = new List<Blocker>();

    public Vector2Int coord;
    public bool IsLoaded => rootObject != null;

    private GameObject rootObject;
    private readonly WorldConfig world;

    private readonly int chunkSize;
    private readonly Transform parent;

    private bool spawnedWithMegaJob = false;

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

    // ================================================================
    // RUNTIME LOAD
    // ================================================================
    public void Load()
    {
        if (IsLoaded) return;

        rootObject = new GameObject($"Chunk_{coord.x}_{coord.y}");
        rootObject.transform.position =
            new Vector3(coord.x * chunkSize, 0, coord.y * chunkSize);

        if (parent != null)
            rootObject.transform.SetParent(parent, false);

        GenerateLOD();
        RunMegaSpawn();
    }

    public void LoadImmediate()
    {
        if (IsLoaded) return;

        rootObject = new GameObject($"Chunk_{coord.x}_{coord.y}");
        rootObject.transform.position =
            new Vector3(coord.x * chunkSize, 0, coord.y * chunkSize);

        if (parent != null)
            rootObject.transform.SetParent(parent, false);

        GenerateImmediateMesh();
        RunMegaSpawn();
    }

    // ================================================================
    // TERRAIN MESH (LOD0/1/2)
    // ================================================================
    private void GenerateLOD()
    {
        var blend = world.GetDominantBiome(coord);
        BiomeConfig biome = blend.biome;

        // Mesh resolutions
        int res0 = chunkSize;
        int res1 = Mathf.Max(2, chunkSize / 2);
        int res2 = Mathf.Max(2, chunkSize / 4);

        // Prepare chunk offset (world pos)
        Vector3 chunkOffset = new Vector3(coord.x * chunkSize, 0, coord.y * chunkSize);

        Mesh lod0 = TerrainMeshGenerator.GenerateMeshSync(
            coord,
            chunkSize,
            chunkSize,
            world,
            biome.useLowPoly
        );

        Mesh lod1 = TerrainMeshGenerator.GenerateMeshSync(
            coord,
            chunkSize,
            chunkSize / 2,
            world,
            biome.useLowPoly
        );

        Mesh lod2 = TerrainMeshGenerator.GenerateMeshSync(
            coord,
            chunkSize,
            chunkSize / 4,
            world,
            biome.useLowPoly
        );


        BurstMeshUtility.RecalculateNormalsBurst(lod0);
        BurstMeshUtility.RecalculateNormalsBurst(lod1);
        BurstMeshUtility.RecalculateNormalsBurst(lod2);

        var renderObj = new GameObject("Mesh_LOD");
        renderObj.transform.SetParent(rootObject.transform, false);
        renderObj.layer = LayerMask.NameToLayer("Default");

        var mf = renderObj.AddComponent<MeshFilter>();
        var mr = renderObj.AddComponent<MeshRenderer>();
        mr.sharedMaterial = biome.groundMaterial;

        mf.sharedMesh = lod0;

        var lodComp = renderObj.AddComponent<ChunkMeshLOD>();
        lodComp.lod0Mesh = lod0;
        lodComp.lod1Mesh = lod1;
        lodComp.lod2Mesh = lod2;
        lodComp.lod1Distance = 80f;
        lodComp.lod2Distance = 160f;

        var colliderObj = new GameObject("Mesh_Collider_LOD0");
        colliderObj.transform.SetParent(rootObject.transform, false);
        colliderObj.layer = LayerMask.NameToLayer("Default");

        var mc = colliderObj.AddComponent<MeshCollider>();
        mc.sharedMesh = lod0;
        mc.cookingOptions =
            MeshColliderCookingOptions.EnableMeshCleaning |
            MeshColliderCookingOptions.CookForFasterSimulation |
            MeshColliderCookingOptions.WeldColocatedVertices;

        colliderObj.AddComponent<ChunkColliderLODFixed>().physicsMesh = lod0;

        if (lod0 == null)
            Debug.LogError($"[Chunk {coord}] COLLIDER LOD0 MESH IS NULL!");
    }

    private void GenerateImmediateMesh()
    {
        var biome = world.GetBiomeAtChunk(coord);
        Vector3 chunkOffset = new Vector3(coord.x * chunkSize, 0, coord.y * chunkSize);

        Mesh m = TerrainMeshGenerator.GenerateMeshSync(
            coord,
            chunkSize,
            chunkSize,
            world,
            biome.useLowPoly
        );

        var go = new GameObject("Mesh");
        go.transform.SetParent(rootObject.transform, false);

        var mf = go.AddComponent<MeshFilter>();
        var mr = go.AddComponent<MeshRenderer>();
        mr.sharedMaterial = biome.groundMaterial;

        mf.sharedMesh = m;

        go.AddComponent<MeshCollider>().sharedMesh = m;
    }

    // ================================================================
    // MEGA SPAWN
    // ================================================================
    private void RunMegaSpawn()
    {
        if (coord.x == 0 && coord.y == 0)
        {
            spawnedWithMegaJob = true;
            return;
        }
        if (spawnedWithMegaJob)
            return;

        spawnedWithMegaJob = true;

        if (!BiomeRuntimeDatabase.Initialized)
        {
            Debug.LogWarning($"[Chunk {coord}] BiomeRuntimeDatabase not initialized.");
            return;
        }

        var biomeCfg = world.GetBiomeAtChunk(coord);
        if (biomeCfg == null)
            return;

        int biomeIndex = -1;
        var layers = world.biomes;
        for (int i = 0; i < layers.Length; i++)
        {
            if (layers[i].config == biomeCfg)
            {
                biomeIndex = i;
                break;
            }
        }

        BiomeParams biomeParams = BiomeRuntimeDatabase.BiomeParamsArray[biomeIndex];

        var lodComp = rootObject.GetComponentInChildren<ChunkMeshLOD>();
        if (lodComp == null || lodComp.lod0Mesh == null)
        {
            Debug.LogError($"[Chunk {coord}] No LOD0 mesh for MegaSpawn!");
            return;
        }

        Mesh lod0 = lodComp.lod0Mesh;
        Vector3[] vertsManaged = lod0.vertices;
        int vertCount = vertsManaged.Length;
        if (vertCount == 0)
            return;

        var vertices = new NativeArray<float3>(vertCount, Allocator.TempJob);
        for (int i = 0; i < vertCount; i++)
            vertices[i] = vertsManaged[i];

        const int sampleStep = 4;

        int maxPerVertex =
            biomeParams.envRuleCount +
            biomeParams.resRuleCount +
            biomeParams.enemyRuleCount +
            biomeParams.questRuleCount;

        if (maxPerVertex <= 0) maxPerVertex = 1;

        int sampledVertices = (vertCount + sampleStep - 1) / sampleStep;
        int estimatedCapacity = math.max(128, sampledVertices * maxPerVertex);

        var spawnList = new NativeList<SpawnInstance>(estimatedCapacity, Allocator.TempJob);

        float3 chunkOffset = new float3(coord.x * chunkSize, 0f, coord.y * chunkSize);

        var job = new MegaSpawnJob
        {
            vertices = vertices,
            biome = biomeParams,
            envRules = BiomeRuntimeDatabase.EnvRules,
            resRules = BiomeRuntimeDatabase.ResRules,
            enemyRules = BiomeRuntimeDatabase.EnemyRules,
            questRules = BiomeRuntimeDatabase.QuestRules,
            output = spawnList.AsParallelWriter(),
            randomSeed = (uint)(coord.x * 73856093 ^ coord.y * 19349663),
            sampleStep = sampleStep,
            vertsPerLine = chunkSize + 1,
            chunkOffset = chunkOffset
        };

        JobHandle handle = job.Schedule(vertCount, 64);

        if (MegaSpawnScheduler.Instance == null)
        {
            var go = new GameObject("MegaSpawnScheduler");
            go.AddComponent<MegaSpawnScheduler>();
        }

        MegaSpawnScheduler.Instance.Schedule(
            coord,
            handle,
            spawnList,
            vertices,
            rootObject
        );
    }

    // ================================================================
    // UNLOAD
    // ================================================================
    public void Unload(int unloadDist, Vector2Int playerChunk)
    {
        if (!IsLoaded) return;

        int dx = Mathf.Abs(coord.x - playerChunk.x);
        int dy = Mathf.Abs(coord.y - playerChunk.y);

        if (dx > unloadDist || dy > unloadDist)
        {
            ChunkedGameObjectStorage.Unload(coord);
            Object.Destroy(rootObject);
            rootObject = null;
        }
    }
}
