using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Biomes.Domain;
using Biomes.Data;

namespace Biomes.UnityIntegration
{
    [BurstCompile]
    public struct MegaSpawnJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<float3> vertices;
        [ReadOnly] public BiomeParams biome;

        [ReadOnly] public NativeArray<EnvironmentRule> envRules;
        [ReadOnly] public NativeArray<ResourceRule>    resRules;
        [ReadOnly] public NativeArray<EnemyRule>       enemyRules;
        [ReadOnly] public NativeArray<QuestRule>       questRules;

        public NativeList<SpawnInstance>.ParallelWriter output;

        public uint   randomSeed;
        public int    sampleStep;
        public int    vertsPerLine;
        public float3 chunkOffset;
        public float2 safeCenter;
        public float safeFlatRadius;
        public float safeBlendRadius;
        public int spawnResources;
        public int spawnEnemies;
        public int spawnQuests;

        private static bool IsFinite(float3 v) =>
            math.isfinite(v.x) && math.isfinite(v.y) && math.isfinite(v.z);

        private float SampleHeight(float3 w)
        {
            float x = w.x;
            float z = w.z;

            int ix = (int)math.floor(x);
            int iz = (int)math.floor(z);

            float tx = x - ix;
            float tz = z - iz;

            ix = math.clamp(ix, 0, vertsPerLine - 2);
            iz = math.clamp(iz, 0, vertsPerLine - 2);

            float h00 = vertices[(iz)     * vertsPerLine + ix].y;
            float h10 = vertices[(iz)     * vertsPerLine + ix + 1].y;
            float h01 = vertices[(iz + 1) * vertsPerLine + ix].y;
            float h11 = vertices[(iz + 1) * vertsPerLine + ix + 1].y;

            float hx0 = math.lerp(h00, h10, tx);
            float hx1 = math.lerp(h01, h11, tx);

            return math.lerp(hx0, hx1, tz);
        }

        private float3 SampleNormalBilinear(float3 w)
        {
            const float eps = 0.5f;

            float hL = SampleHeight(new float3(w.x - eps, 0, w.z));
            float hR = SampleHeight(new float3(w.x + eps, 0, w.z));
            float hD = SampleHeight(new float3(w.x, 0, w.z - eps));
            float hU = SampleHeight(new float3(w.x, 0, w.z + eps));

            float3 dx = new float3(2 * eps, hR - hL, 0);
            float3 dz = new float3(0, hU - hD, 2 * eps);

            float3 n = math.cross(dz, dx);
            if (math.lengthsq(n) < 1e-6f)
                return new float3(0, 1, 0);

            n = math.normalize(n);
            if (!math.isfinite(n.x) || !math.isfinite(n.y) || !math.isfinite(n.z))
                return new float3(0, 1, 0);

            if (n.y < 0) n = -n;
            return n;
        }

        private float ComputeSlope(float3 normal)
        {
            float dot = math.dot(normal, new float3(0, 1, 0));
            dot = math.clamp(dot, -1f, 1f);
            return math.degrees(math.acos(dot));
        }

        private float ComputeSafeFactor(float2 worldXZ)
        {
            float flatRadius = math.max(0f, safeFlatRadius);
            float blendRadius = math.max(flatRadius, safeBlendRadius);
            float dist = math.distance(worldXZ, safeCenter);

            if (dist < flatRadius)
                return 0f;

            if (dist >= blendRadius || blendRadius <= flatRadius)
                return 1f;

            float t = (dist - flatRadius) / (blendRadius - flatRadius);
            return t * t * (3f - 2f * t);
        }

        // ==============================
        // MAIN
        // ==============================

        public void Execute(int index)
        {
            if ((index % sampleStep) != 0)
                return;
            
            uint seed = randomSeed + (uint)index * 1664525u;
            if (seed == 0) seed = 1;
            var rng = new Random(seed);

            float3 localVert = vertices[index];
            if (!IsFinite(localVert)) return;

            float3 localPos = localVert;
            localPos.y = SampleHeight(localPos);

            float3 normal = SampleNormalBilinear(localPos);
            float  slope  = ComputeSlope(normal);

            float3 worldBasePos = localPos + chunkOffset;
            float safeFactor = ComputeSafeFactor(worldBasePos.xz);
            if (safeFactor <= 0f)
                return;

            // ---------------------------------
            // ENVIRONMENT INSTANCED
            // ---------------------------------
            for (int i = 0; i < biome.envRuleCount; i++)
            {
                var r = envRules[biome.envRuleStart + i];

                if (rng.NextFloat() > r.spawnChance * safeFactor) continue;
                if (slope < r.minSlope || slope > r.maxSlope) continue;

                float scale = math.lerp(r.minScale, r.maxScale, rng.NextFloat());
                int flags = 0;
                if (r.alignToNormal != 0)
                    flags |= SpawnInstanceFlags.AlignToNormal;
                if (r.randomYRotation != 0)
                    flags |= SpawnInstanceFlags.RandomYRotation;

                output.AddNoResize(new SpawnInstance
                {
                    position    = worldBasePos,
                    normal      = normal,
                    prefabIndex = r.prefabIndex,
                    biomeId     = biome.biomeId,
                    scale       = scale,
                    extraData   = flags,
                    spawnType   = (int)SpawnKind.EnvironmentInstanced,
                    random01    = rng.NextFloat()
                });
            }

            // ---------------------------------
            // RESOURCES (clusters)
            // ---------------------------------
            if (spawnResources != 0)
            {
                for (int i = 0; i < biome.resRuleCount; i++)
                {
                    var r = resRules[biome.resRuleStart + i];
                    SpawnResource(ref rng, r, localPos, normal, slope, safeFactor);
                }
            }

            // ---------------------------------
            // ENEMIES
            // ---------------------------------
            if (spawnEnemies != 0)
            {
                for (int i = 0; i < biome.enemyRuleCount; i++)
                {
                    var r = enemyRules[biome.enemyRuleStart + i];

                    if (rng.NextFloat() > r.spawnChance * safeFactor) continue;
                    if (slope < r.minSlope || slope > r.maxSlope) continue;

                    output.AddNoResize(new SpawnInstance
                    {
                        position    = worldBasePos,
                        normal      = normal,
                        prefabIndex = r.prefabIndex,
                        biomeId     = biome.biomeId,
                        scale       = 1f,
                        spawnType   = (int)SpawnKind.EnemyGameObject
                    });
                }
            }

            // ---------------------------------
            // QUESTS
            // ---------------------------------
            if (spawnQuests != 0)
            {
                for (int i = 0; i < biome.questRuleCount; i++)
                {
                    var r = questRules[biome.questRuleStart + i];

                    if (rng.NextFloat() > r.spawnChance * safeFactor) continue;

                    output.AddNoResize(new SpawnInstance
                    {
                        position    = worldBasePos,
                        normal      = normal,
                        prefabIndex = r.prefabIndex,
                        biomeId     = biome.biomeId,
                        scale       = 1f,
                        spawnType   = (int)SpawnKind.QuestGameObject
                    });
                }
            }
        }

        // ==============================
        // RESOURCES CLUSTERS
        // ==============================

        private void SpawnResource(
            ref Random rng,
            ResourceRule r,
            float3 localBasePos,
            float3 baseNormal,
            float slope,
            float safeFactor)
        {
            if (rng.NextFloat() > r.spawnChance * safeFactor) return;
            if (slope < r.minSlope || slope > r.maxSlope) return;

            int count =
                r.clusterType == ResourceClusterType.Single
                ? 1
                : rng.NextInt(math.max(1, r.clusterMin),
                              math.max(r.clusterMin, r.clusterMax) + 1);

            float3 up = baseNormal;
            float3 right = math.cross(up, new float3(0, 0, 1));
            if (math.lengthsq(right) < 0.001f)
                right = math.cross(up, new float3(1, 0, 0));
            right = math.normalize(right);

            float3 forward = math.normalize(math.cross(up, right));

            for (int i = 0; i < count; i++)
            {
                float3 offset = float3.zero;

                if (r.clusterType == ResourceClusterType.RoundCluster)
                {
                    float ang = rng.NextFloat(0, math.PI * 2);
                    float rad = rng.NextFloat(0, r.clusterRadius);
                    offset = right * math.cos(ang) * rad +
                             forward * math.sin(ang) * rad;
                }

                float3 localP = localBasePos + offset;
                localP.y = SampleHeight(localP);

                float3 normal = SampleNormalBilinear(localP);

                float scale = r.randomScale != 0
                    ? math.lerp(r.minScale, r.maxScale, rng.NextFloat())
                    : r.minScale;
                if (scale <= 0f || !math.isfinite(scale))
                    scale = 1f;

                float3 worldP = localP + chunkOffset;
                if (ComputeSafeFactor(worldP.xz) <= 0f)
                    continue;

                output.AddNoResize(new SpawnInstance
                {
                    position    = worldP,
                    normal      = normal,
                    prefabIndex = r.prefabIndex,
                    biomeId     = biome.biomeId,
                    scale       = scale,
                    spawnType   = (int)SpawnKind.ResourceGameObject
                });
            }
        }
    }
}
