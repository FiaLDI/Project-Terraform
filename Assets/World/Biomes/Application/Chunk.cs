using UnityEngine;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using Biomes.Application;
using Unity.Jobs;
using Biomes.Domain;
using Biomes.UnityIntegration;
using Biomes.Data;
using Biomes.Utility;
using FishNet;

public class Chunk
{
    public List<Blocker> environmentBlockers = new();

    public Vector2Int coord;
    public bool IsLoaded => rootObject != null;

    private GameObject rootObject;
    private readonly WorldConfig world;

    private readonly int chunkSize;
    private readonly Transform parent;

    private bool spawnedWithMegaJob = false;

    private readonly List<Mesh> _runtimeMeshes = new();

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

    public void Load()
    {
        if (IsLoaded) 
            return;

        rootObject = new GameObject($"Chunk_{coord.x}_{coord.y}");
        rootObject.transform.position = new Vector3(coord.x * chunkSize, 0, coord.y * chunkSize);

        if (parent != null)
            rootObject.transform.SetParent(parent, false);

        GenerateLOD();
        RunMegaSpawn();
    }
    public void LoadImmediate()
    {
        if (IsLoaded) 
            return;

        rootObject = new GameObject($"Chunk_{coord.x}_{coord.y}");
        rootObject.transform.position = new Vector3(coord.x * chunkSize, 0, coord.y * chunkSize);

        if (parent != null)
            rootObject.transform.SetParent(parent, false);

        GenerateImmediateMesh();
        RunMegaSpawn();
    }

    // ================================================================
    // TERRAIN GENERATION (LOD0/1/2)
    // ================================================================
    private void GenerateLOD()
    {
        var blend = world.GetDominantBiome(coord);
        BiomeConfig biome = blend.biome;

        // =========================
        // LOD0
        // =========================
        MeshData data0 = MeshDataGenerator.GenerateData(
            coord,
            chunkSize,
            chunkSize,
            world,
            biome.useLowPoly
        );

        Mesh lod0 = TerrainMeshGenerator.BuildMesh(data0);
        lod0.MarkDynamic();
        _runtimeMeshes.Add(lod0);

        // =========================
        // LOD1
        // =========================
        MeshData data1 = MeshDataGenerator.GenerateData(
            coord,
            chunkSize,
            chunkSize / 2,
            world,
            biome.useLowPoly
        );

        Mesh lod1 = TerrainMeshGenerator.BuildMesh(data1);
        lod1.MarkDynamic();
        _runtimeMeshes.Add(lod1);

        // =========================
        // LOD2
        // =========================
        MeshData data2 = MeshDataGenerator.GenerateData(
            coord,
            chunkSize,
            chunkSize / 4,
            world,
            biome.useLowPoly
        );

        Mesh lod2 = TerrainMeshGenerator.BuildMesh(data2);
        lod2.MarkDynamic();
        _runtimeMeshes.Add(lod2);

        // =========================
        // Нормали (Burst)
        // =========================
        BurstMeshUtility.RecalculateNormalsBurst(lod0);
        BurstMeshUtility.RecalculateNormalsBurst(lod1);
        BurstMeshUtility.RecalculateNormalsBurst(lod2);

        // ВАЖНО: финализируем данные перед физикой
        lod0.UploadMeshData(false);

        // =========================
        // RENDER OBJECT
        // =========================
        var renderObj = new GameObject("Mesh_LOD");
        renderObj.transform.SetParent(rootObject.transform, false);
        renderObj.layer = LayerMask.NameToLayer("Ground");

        var mf = renderObj.AddComponent<MeshFilter>();
        var mr = renderObj.AddComponent<MeshRenderer>();

        BiomeMaterialUtility.ApplyBiomeMaterial(mr, biome, world);

        mf.sharedMesh = lod0;

        var lodComp = renderObj.AddComponent<ChunkMeshLOD>();
        lodComp.lod0Mesh = lod0;
        lodComp.lod1Mesh = lod1;
        lodComp.lod2Mesh = lod2;
        lodComp.lod1Distance = 80f;
        lodComp.lod2Distance = 160f;

        // =========================
        // COLLIDER OBJECT
        // =========================
        var colliderObj = new GameObject("Mesh_Collider_LOD0");
        colliderObj.transform.SetParent(rootObject.transform, false);
        colliderObj.layer = LayerMask.NameToLayer("Default");

        var mc = colliderObj.AddComponent<MeshCollider>();

        // Принудительный ре-кукинг
        mc.sharedMesh = null;
        mc.sharedMesh = lod0;
    }

    private void GenerateImmediateMesh()
    {
        var biome = world.GetBiomeAtChunk(coord);

        Mesh m = TerrainMeshGenerator.GenerateMeshSync(
            coord,
            chunkSize,
            chunkSize,
            world,
            biome.useLowPoly
        );

        m.MarkDynamic();
        _runtimeMeshes.Add(m);

        BurstMeshUtility.RecalculateNormalsBurst(m);
        m.UploadMeshData(false);

        var go = new GameObject("Mesh");
        go.transform.SetParent(rootObject.transform, false);

        var mf = go.AddComponent<MeshFilter>();
        var mr = go.AddComponent<MeshRenderer>();
        BiomeMaterialUtility.ApplyBiomeMaterial(mr, biome, world);

        mf.sharedMesh = m;

        var mc = go.AddComponent<MeshCollider>();
        mc.sharedMesh = null;
        mc.sharedMesh = m;
    }


    // ================================================================
    // MEGA SPAWN
    // ================================================================
    private void RunMegaSpawn()
    {
        // 1) чтобы не запускать дважды
        if (spawnedWithMegaJob)
            return;

        spawnedWithMegaJob = true;

        // 2) Проверяем, инициализирована ли база биомов
        if (!BiomeRuntimeDatabase.Initialized)
        {
            Debug.LogWarning($"[Chunk {coord}] BiomeRuntimeDatabase not initialized, skip MegaSpawn");
            return;
        }

        // 3) Узнаём биом чанка
        var biomeCfg = world.GetBiomeAtChunk(coord);
        if (biomeCfg == null)
        {
            Debug.LogWarning($"[Chunk {coord}] GetBiomeAtChunk returned NULL, skip MegaSpawn");
            return;
        }

        // 4) Находим индекс этого биома в WorldConfig.biomes
        int biomeIndex = -1;
        var layers = world.biomes;

        if (layers == null || layers.Length == 0)
        {
            Debug.LogWarning($"[Chunk {coord}] World has no biomes, skip MegaSpawn");
            return;
        }

        for (int i = 0; i < layers.Length; i++)
        {
            if (layers[i].config == biomeCfg)
            {
                biomeIndex = i;
                break;
            }
        }

        if (biomeIndex < 0)
        {
            Debug.LogError($"[Chunk {coord}] Cannot find biome index for biome '{biomeCfg.biomeName}' in WorldConfig.biomes");
            return;
        }

        if (BiomeRuntimeDatabase.BiomeParamsArray == null ||
            biomeIndex >= BiomeRuntimeDatabase.BiomeParamsArray.Length)
        {
            Debug.LogError(
                $"[Chunk {coord}] biomeIndex={biomeIndex} is out of range " +
                $"for BiomeParamsArray (len={BiomeRuntimeDatabase.BiomeParamsArray?.Length ?? 0})"
            );
            return;
        }

        BiomeParams biomeParams = BiomeRuntimeDatabase.BiomeParamsArray[biomeIndex];

        // 5) Берём LOD0 меш
        if (rootObject == null)
        {
            Debug.LogError($"[Chunk {coord}] rootObject is NULL in MegaSpawn");
            return;
        }

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

        // 6) Готовим данные для джоба
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

        // 7) Сид мира + координата чанка → уникальный сид
        uint baseSeed  = (uint)world.seed;
        uint finalSeed = baseSeed
                        ^ (uint)(coord.x * 73856093)
                        ^ (uint)(coord.y * 19349663);
        bool spawnRuntimeGameObjects = InstanceFinder.IsServerStarted;

        var job = new MegaSpawnJob
        {
            vertices     = vertices,
            biome        = biomeParams,
            envRules     = BiomeRuntimeDatabase.EnvRules,
            resRules     = BiomeRuntimeDatabase.ResRules,
            enemyRules   = BiomeRuntimeDatabase.EnemyRules,
            questRules   = BiomeRuntimeDatabase.QuestRules,
            output       = spawnList.AsParallelWriter(),
            randomSeed   = finalSeed,
            sampleStep   = sampleStep,
            vertsPerLine = chunkSize + 1,
            chunkOffset  = chunkOffset,
            safeCenter   = world.GetSafeSpawnCenter(),
            safeFlatRadius = world.safeSpawnFlatRadius,
            safeBlendRadius = world.safeSpawnBlendRadius,
            spawnResources = spawnRuntimeGameObjects ? 1 : 0,
            spawnEnemies = spawnRuntimeGameObjects ? 1 : 0,
            spawnQuests = spawnRuntimeGameObjects ? 1 : 0,
        };

        JobHandle handle = job.Schedule(vertCount, 64);

        // 8) Планировщик, который потом Dispose-ит spawnList/vertices
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
    // UNLOAD — MUST DESTROY ALL MESHES!
    // ================================================================
    public void Unload(int unloadDist, Vector2Int playerChunk)
    {
        // В новой схеме ChunkManager уже решил, что чанк надо снести.
        if (!IsLoaded) 
            return;

        // 1) выгружаем все спавн-объекты этого чанка (ресурсы, враги, квесты)
        ChunkedGameObjectStorage.Unload(coord);

        // 2) чистим блокеры, чтобы не держать ссылки на уничтоженные компоненты
        environmentBlockers.Clear();

        // 3) уничтожаем ВСЕ runtime-меши (включая те, что висят на MeshFilter/MeshCollider)
        foreach (var mesh in _runtimeMeshes)
        {
            if (mesh == null) 
                continue;

            mesh.Clear();
            Object.Destroy(mesh);
        }
        _runtimeMeshes.Clear();

        // 4) уничтожаем корневой объект чанка (с MeshFilter, MeshRenderer, MeshCollider, ChunkMeshLOD и т.д.)
        Object.Destroy(rootObject);
        rootObject = null;

        // 5) сбрасываем флаг спавна, чтобы при повторной загрузке можно было снова вызвать RunMegaSpawn()
        spawnedWithMegaJob = false;
    }
}
