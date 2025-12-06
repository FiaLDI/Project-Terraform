using UnityEngine;
using Features.Quests.Data;
using Features.Enemy.Data;

namespace Features.Biomes.Domain
{
    public enum TerrainType
    {
        SmoothHills,
        SharpMountains,
        Plateaus,
        Craters,
        Dunes,
        Islands,
        Canyons,
        FractalMountains
    }

    public enum WaterType
    {
        None,
        Ocean,
        Lake,
        Swamp
    }

    // ---------- ENVIRONMENT ENTRY ----------
    [System.Serializable]
    public class EnvironmentEntry
    {
        public GameObject prefab;

        [Range(0f, 1f)] public float spawnChance = 1f;
        public float weight = 1f;

        [Header("Slope")]
        [Range(0f, 90f)] public float minSlope = 0f;
        [Range(0f, 90f)] public float maxSlope = 45f;

        [Header("Rotation")]
        public bool alignToNormal = true;
        public bool randomYRotation = true;

        [Header("Scale")]
        public bool randomScale = false;
        public float minScale = 0.9f;
        public float maxScale = 1.1f;

        [Header("Resource blocking")]
        public bool markAsResourceBlocker = true;
    }

    // ---------- QUEST ENTRY ----------
    [System.Serializable]
    public class QuestEntry
    {
        public QuestAsset questAsset;
        public GameObject questPointPrefab;

        [Range(0f, 1f)] public float spawnChance = 1f;

        public int spawnPointsMin = 1;
        public int spawnPointsMax = 3;

        [Min(1)] public int requiredTargets = 1;

        public float safetyRadius = 5f;
    }

    // ---------- ENEMY ENTRY ----------
    [System.Serializable]
    public class EnemySpawnEntry
    {
        public EnemyConfigSO config;

        public float weight = 1f;
        public float spawnChance = 0.6f;
        public int minGroup = 1;
        public int maxGroup = 3;

        public float minSlope = 0f;
        public float maxSlope = 25f;
        public float minHeight = -100f;
        public float maxHeight = 500f;

        public bool alignToNormal = true;
    }

    public enum ResourceClusterType
    {
        Single,
        CrystalVein,
        RoundCluster,
        VerticalStackNoise
    }

    [System.Serializable]
    public class ResourceEntry
    {
        [Header("Prefab")]
        public GameObject resourcePrefab;

        [Header("Spawn Settings")]
        [Range(0f, 1f)] public float spawnChance = 1f;
        [Range(0f, 10f)] public float weight = 1f;

        [Header("Cluster Settings")]
        public ResourceClusterType clusterType = ResourceClusterType.Single;

        public Vector2Int clusterCountRange = new Vector2Int(1, 10);
        public float clusterRadius = 3f;
        public float verticalStep = 0.8f;

        [Header("Noise Settings")]
        public float noiseScale = 0.5f;
        public float noiseAmplitude = 2f;

        [Header("Terrain Fit")]
        public bool followTerrain = true;

        [Header("Slope Rules")]
        [Range(0f, 90f)] public float minSlope = 0f;
        [Range(0f, 90f)] public float maxSlope = 45f;

        [Header("Rotation Rules")]
        public bool alignToNormal = true;
        public bool randomYRotation = true;

        [Header("Scale Rules")]
        public bool randomScale = true;
        public float minScale = 0.9f;
        public float maxScale = 1.4f;

        [Header("Height Rules")]
        public bool useHeightLimit = false;
        public float minHeight = 0f;
        public float maxHeight = 999f;

        [Header("Spacing Rules")]
        public bool useMinDistance = false;
        public float minDistance = 2f;

        [Header("Resource Edge Falloff")]
        [Range(0f, 1f)] public float resourceEdgeFalloff = 0.5f;

        [Header("Environment Exclusion")]
        public bool avoidEnvironment = true;
        public float environmentRadius = 3f;

        [Header("Distribution Map")]
        public bool useDistributionMap = true;
        [Range(0f, 2f)] public float distributionStrength = 1f;
        public float distributionScale = 0.05f;
        public Vector2 distributionOffset;

        [Header("Biome Rules")]
        public TerrainType[] allowedBiomes;  // <-- теперь 100% сериализуется
    }

    // ---------- BIOME CONFIG ----------
    [CreateAssetMenu(menuName = "Game/Biome Config")]
    public class BiomeConfig : ScriptableObject
    {
        [Header("Info")]
        public string biomeName;
        public Color mapColor = Color.white;

        [Header("Usage")]
        public bool isGenerate = true;
        public bool useLowPoly = false;

        [Header("Terrain")]
        public TerrainType terrainType = TerrainType.SmoothHills;

        [Header("Texture / UV")]
        public float textureTiling = 1f;
        public Material groundMaterial;
        public float terrainScale = 10f;
        public float heightMultiplier = 5f;

        [Header("Fractal Mountains")]
        [Range(1, 8)] public int fractalOctaves = 5;
        [Range(0.1f, 1f)] public float fractalPersistence = 0.5f;
        [Range(1.5f, 4f)] public float fractalLacunarity = 2f;

        [Header("Blending")]
        [Range(0f, 1f)] public float blendStrength = 0f;

        [Header("Environment Objects")]
        public EnvironmentEntry[] environmentPrefabs;
        [Range(0f, 1f)] public float environmentDensity = 0.05f;

        [Header("Resources")]
        public ResourceEntry[] possibleResources;
        [Range(0f, 0.01f)] public float resourceDensity = 0.001f;
        public float resourceSpawnYOffset = 0.3f;
        [Range(0f, 1f)] public float resourceEdgeFalloff = 1f;

        [Header("Quests")]
        public QuestEntry[] possibleQuests;
        public int questTargetsMin = 2;
        public int questTargetsMax = 5;

        // -------- ENEMIES ---------
        [Header("Enemies")]
        public EnemySpawnEntry[] enemyTable;
        public float enemyDensity = 0.0008f;
        public float enemyRespawnDelay = 20f;

        // ---------- SKYBOX / UI / FOG ----------
        [Header("Skybox Settings")]
        public Material skyboxMaterial;
        public Color skyTopColor = Color.white;
        public Color skyBottomColor = Color.gray;
        public float skyExposure = 1f;

        [Header("UI / Fog Gradient")]
        public Color uiColor = Color.white;
        public Color fogLightColor = Color.white;
        public Color fogHeavyColor = Color.blue;
        public float fogGradientScale = 10f;

        [Header("Fog")]
        public bool enableFog = true;
        public FogMode fogMode = FogMode.Exponential;
        public Color fogColor = Color.white;
        public float fogDensity = 0.02f;
        public float fogLinearStart = 0f;
        public float fogLinearEnd = 200f;

        // ---------- WEATHER ----------
        [Header("Weather")]
        public GameObject rainPrefab;
        public GameObject dustPrefab;
        public GameObject firefliesPrefab;
        [Range(0f, 1f)] public float weatherIntensity = 1f;

        // ---------- WATER ----------
        [Header("Water")]
        public bool useWater = false;
        public WaterType waterType = WaterType.Ocean;
        public float seaLevel = 1f;
        public Material waterMaterial;
        public Material swampWaterMaterial;
        public Material lakeWaterMaterial;
        public Material oceanWaterMaterial;

        // ---------- LAKES ----------
        [Header("Lakes")]
        public bool generateLakes = false;
        public float lakeLevel = 0.6f;
        public float lakeNoiseScale = 0.05f;

        // ---------- RIVERS ----------
        [Header("Rivers")]
        public bool generateRivers = false;
        public float riverNoiseScale = 0.02f;
        [Range(0f, 0.5f)] public float riverWidth = 0.1f;
        public float riverDepth = 2f;

        [Header("Biome Area Size")]
        public int width = 100;
        public int height = 100;
    }
}
