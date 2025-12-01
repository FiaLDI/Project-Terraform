using Unity.Collections;
using UnityEngine;
using Features.Biomes.Domain;
using System.Collections.Generic;
using Unity.Mathematics;

namespace Features.Biomes.Application
{
    public static class BiomeRuntimeDatabase
    {
        public static BiomeParams[] BiomeParamsArray;

        public static NativeArray<EnvironmentRule> EnvRules;
        public static NativeArray<ResourceRule>    ResRules;
        public static NativeArray<EnemyRule>       EnemyRules;
        public static NativeArray<QuestRule>       QuestRules;

        public static bool Initialized => BiomeParamsArray != null;

        public static void Build(WorldConfig world)
        {
            var layers = world.biomes;

            List<EnvironmentRule> env = new();
            List<ResourceRule>    res = new();
            List<EnemyRule>       ene = new();
            List<QuestRule>       que = new();

            BiomeParamsArray = new BiomeParams[layers.Length];

            for (int i = 0; i < layers.Length; i++)
            {
                BiomeConfig cfg = layers[i].config;

                BiomeParams p = new BiomeParams
                {
                    biomeId              = i,
                    resourceSpawnYOffset = cfg.resourceSpawnYOffset,
                    resourceDensity      = cfg.resourceDensity,
                    resourceEdgeFalloff  = cfg.resourceEdgeFalloff,
                    enemyDensity         = cfg.enemyDensity,
                    enemyRespawnDelay    = cfg.enemyRespawnDelay,
                    environmentDensity   = cfg.environmentDensity
                };

                // ----------------------
                // ENVIRONMENT → instanced
                // ----------------------
                p.envRuleStart = env.Count;
                foreach (var entry in cfg.environmentPrefabs)
                {
                    if (entry.prefab == null) continue;

                    InstanceRegistry.Register(entry.prefab, allowInstancing: true);

                    env.Add(new EnvironmentRule
                    {
                        prefabIndex      = entry.prefab.GetInstanceID(),
                        spawnChance      = cfg.environmentDensity * entry.spawnChance,
                        minSlope         = entry.minSlope,
                        maxSlope         = entry.maxSlope,
                        minScale         = entry.minScale,
                        maxScale         = entry.maxScale,
                        alignToNormal    = entry.alignToNormal ? 1 : 0,
                        randomYRotation  = entry.randomYRotation ? 1 : 0,
                        markAsBlocker    = entry.markAsResourceBlocker ? 1 : 0
                    });
                }
                p.envRuleCount = env.Count - p.envRuleStart;

                // ----------------------
                // RESOURCES → GameObject
                // ----------------------
                p.resRuleStart = res.Count;
                foreach (var entry in cfg.possibleResources)
                {
                    if (entry.resourcePrefab == null) continue;

                    // ресурсы как GameObject, instancing не нужен
                    InstanceRegistry.Register(entry.resourcePrefab, allowInstancing: false);

                    float minSlope = entry.minSlope;
                    float maxSlope = entry.maxSlope;
                    if (minSlope == 0f && maxSlope == 0f)
                    {
                        minSlope = 0f;
                        maxSlope = 90f;
                    }

                    var rule = new ResourceRule
                    {
                        prefabIndex = entry.resourcePrefab.GetInstanceID(),

                        spawnChance = cfg.resourceDensity * entry.spawnChance,
                        weight      = entry.weight,

                        clusterType   = entry.clusterType,
                        clusterMin    = entry.clusterCountRange.x,
                        clusterMax    = entry.clusterCountRange.y,
                        clusterRadius = entry.clusterRadius,
                        verticalStep  = entry.verticalStep,

                        noiseScale     = entry.noiseScale,
                        noiseAmplitude = entry.noiseAmplitude,
                        followTerrain  = entry.followTerrain ? 1 : 0,

                        minSlope = minSlope,
                        maxSlope = maxSlope,

                        alignToNormal   = entry.alignToNormal ? 1 : 0,
                        randomYRotation = entry.randomYRotation ? 1 : 0,

                        randomScale = entry.randomScale ? 1 : 0,
                        minScale    = entry.minScale,
                        maxScale    = entry.maxScale,

                        useHeightLimit = entry.useHeightLimit ? 1 : 0,
                        minHeight      = entry.minHeight,
                        maxHeight      = entry.maxHeight,

                        useMinDistance = entry.useMinDistance ? 1 : 0,
                        minDistance    = entry.minDistance,

                        resourceEdgeFalloff = entry.resourceEdgeFalloff,

                        avoidEnvironment  = entry.avoidEnvironment ? 1 : 0,
                        environmentRadius = entry.environmentRadius,

                        useDistributionMap   = entry.useDistributionMap ? 1 : 0,
                        distributionStrength = entry.distributionStrength,
                        distributionScale    = entry.distributionScale,
                        distributionOffset   = new float2(entry.distributionOffset.x, entry.distributionOffset.y),

                        allowedBiomeStart = -1,
                        allowedBiomeCount = 0
                    };

                    res.Add(rule);
                }
                p.resRuleCount = res.Count - p.resRuleStart;

                // ----------------------
                // ENEMIES → GameObject
                // ----------------------
                p.enemyRuleStart = ene.Count;
                foreach (var entry in cfg.enemyTable)
                {
                    if (entry.prefab == null) continue;

                    InstanceRegistry.Register(entry.prefab, allowInstancing: false);

                    ene.Add(new EnemyRule
                    {
                        prefabIndex   = entry.prefab.GetInstanceID(),
                        spawnChance   = cfg.enemyDensity * entry.spawnChance,
                        minSlope      = entry.minSlope,
                        maxSlope      = entry.maxSlope,
                        minHeight     = entry.minHeight,
                        maxHeight     = entry.maxHeight,
                        minGroup      = entry.minGroup,
                        maxGroup      = entry.maxGroup,
                        alignToNormal = entry.alignToNormal ? 1 : 0
                    });
                }
                p.enemyRuleCount = ene.Count - p.enemyRuleStart;

                // ----------------------
                // QUESTS → GameObject
                // ----------------------
                p.questRuleStart = que.Count;
                foreach (var entry in cfg.possibleQuests)
                {
                    if (entry.questPointPrefab == null) continue;

                    InstanceRegistry.Register(entry.questPointPrefab, allowInstancing: false);

                    que.Add(new QuestRule
                    {
                        prefabIndex     = entry.questPointPrefab.GetInstanceID(),
                        spawnChance     = entry.spawnChance,
                        spawnPointsMin  = entry.spawnPointsMin,
                        spawnPointsMax  = entry.spawnPointsMax,
                        requiredTargets = entry.requiredTargets,
                        safetyRadius    = entry.safetyRadius
                    });
                }
                p.questRuleCount = que.Count - p.questRuleStart;

                BiomeParamsArray[i] = p;
            }

            EnvRules   = new NativeArray<EnvironmentRule>(env.ToArray(), Allocator.Persistent);
            ResRules   = new NativeArray<ResourceRule>(res.ToArray(), Allocator.Persistent);
            EnemyRules = new NativeArray<EnemyRule>(ene.ToArray(), Allocator.Persistent);
            QuestRules = new NativeArray<QuestRule>(que.ToArray(), Allocator.Persistent);

            Debug.Log($"[BiomeRuntimeDatabase] Build completed. Env={EnvRules.Length}, Res={ResRules.Length}, Enemies={EnemyRules.Length}, Quests={QuestRules.Length}");
        }
    }
}
