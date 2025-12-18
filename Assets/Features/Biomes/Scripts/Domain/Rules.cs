using Unity.Mathematics;

namespace Features.Biomes.Domain
{

    public enum SpawnKind
    {
        EnvironmentInstanced = 0,
        ResourceGameObject   = 1, 
        EnemyGameObject      = 2, 
        QuestGameObject      = 3 
    }

    public struct EnvironmentRule
    {
        public int prefabIndex;
        public float spawnChance;

        public float minSlope;
        public float maxSlope;

        public float minScale;
        public float maxScale;

        public int alignToNormal;
        public int randomYRotation;

        public int markAsBlocker;
    }

    public struct ResourceRule
    {
        public int prefabIndex;

        public float spawnChance;
        public float weight;

        public ResourceClusterType clusterType;
        public int clusterMin;
        public int clusterMax;
        public float clusterRadius;
        public float verticalStep;

        public float noiseScale;
        public float noiseAmplitude;

        public int followTerrain;

        public float minSlope;
        public float maxSlope;

        public int alignToNormal;
        public int randomYRotation;

        public int randomScale;
        public float minScale;
        public float maxScale;

        public int useHeightLimit;
        public float minHeight;
        public float maxHeight;

        public int useMinDistance;
        public float minDistance;

        public float resourceEdgeFalloff;

        public int avoidEnvironment;
        public float environmentRadius;

        public int useDistributionMap;
        public float distributionStrength;
        public float distributionScale;
        public float2 distributionOffset;

        // Пока не используем, но оставляем задел
        public int allowedBiomeStart;
        public int allowedBiomeCount;
    }


    public struct EnemyRule
    {
        public int prefabIndex;
        public float spawnChance;

        public float minSlope;
        public float maxSlope;
        public float minHeight;
        public float maxHeight;

        public int minGroup;
        public int maxGroup;

        public int alignToNormal;
    }

    public struct QuestRule
    {
        public int prefabIndex;
        public float spawnChance;
        public int spawnPointsMin;
        public int spawnPointsMax;
        public int requiredTargets;
        public float safetyRadius;
    }

    public struct SpawnInstance
    {
        public float3 position;
        public float3 normal;

        public int prefabIndex;
        public int biomeId;
        public float scale;

        public int extraData;

        public int spawnType;

        public float random01;
        public float4 color;
    }
}
