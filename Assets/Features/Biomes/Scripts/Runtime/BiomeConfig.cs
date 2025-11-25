using UnityEngine;
using Quests;

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

[System.Serializable]
public class EnvironmentEntry
{
    [Header("Base")]
    public GameObject prefab;

    [Range(0f, 1f)]
    public float spawnChance = 1f;

    [Range(0f, 10f)]
    public float weight = 1f;

    [Header("Slope Rules")]
    [Range(0f, 90f)] public float minSlope = 0f;
    [Range(0f, 90f)] public float maxSlope = 45f;

    [Header("Rotation")]
    public bool alignToNormal = true;
    public bool randomYRotation = true;

    [Header("Scale")]
    public bool randomScale = false;
    public float minScale = 0.9f;
    public float maxScale = 1.1f;

    [Header("Resource Blocking")]
    [Tooltip("Если включено, этот объект учитывается как блокер ресурсов (деревья, крупные камни и т.п.)")]
    public bool markAsResourceBlocker = true;
}

[System.Serializable]
public class QuestEntry
{
    public QuestAsset questAsset;
    public GameObject questPointPrefab;

    [Range(0f, 1f)]
    public float spawnChance = 1f;

    public int spawnPointsMin = 1;
    public int spawnPointsMax = 3;

    [Tooltip("Сколько точек нужно выполнить, чтобы завершить квест.")]
    [Min(1)]
    public int requiredTargets = 1;

    public float safetyRadius = 5f;
}

[System.Serializable]
public class EnemySpawnEntry
{
    public GameObject prefab;
    public float weight = 1f;
    public float spawnChance = 0.6f;
    public int minGroup = 1;
    public int maxGroup = 3;

    [Header("Условия спавна")]
    public float minSlope = 0f;
    public float maxSlope = 25f;
    public float minHeight = -100f;
    public float maxHeight = 500f;

    public bool alignToNormal = true;
}


[CreateAssetMenu(menuName = "Game/Biome Config")]
public class BiomeConfig : ScriptableObject
{
    // ===========================
    // BASIC
    // ===========================
    [Header("Основное")]
    public string biomeName;
    public Color mapColor = Color.white;

    [Header("Генерация")]
    public bool isGenerate = true;

    [Header("Rendering")]
    [Tooltip("Если включено — рельеф этого биома будет сгенерирован в Low-Poly стиле.")]
    public bool useLowPoly = false;

    // Размеры (используются BiomeHeightUtility)
    [Header("Размер карты (для озёр/воды)")]
    public int width = 100;
    public int height = 100;

    [Header("UI")]
    public Color uiColor = Color.white;

    [Header("UI Fog Gradient")]
    public Color fogLightColor = Color.white;
    public Color fogHeavyColor = Color.blue;
    public float fogGradientScale = 10f;

    // ===========================
    // TERRAIN
    // ===========================
    [Header("Рельеф")]
    public TerrainType terrainType = TerrainType.SmoothHills;
    public Material groundMaterial;
    public float terrainScale = 10f;
    public float heightMultiplier = 5f;

    // Fractal mountains
    [Header("Fractal Mountains")]
    [Range(1, 8)] public int fractalOctaves = 5;
    [Range(0.1f, 1f)] public float fractalPersistence = 0.5f;
    [Range(1.5f, 4f)] public float fractalLacunarity = 2f;

    // ===========================
    // ENVIRONMENT
    // ===========================
    [Header("Окружение")]
    public EnvironmentEntry[] environmentPrefabs;
    [Range(0f, 1f)] public float environmentDensity = 0.05f;

    // ===========================
    // RESOURCES
    // ===========================
    [Header("Ресурсы")]
    public ResourceEntry[] possibleResources;

    [Range(0f, 0.01f)] 
    public float resourceDensity = 0.001f;

    public float resourceSpawnYOffset = 0.3f;

    [Header("Падение плотности ресурсов к краям чанка")]
    [Range(0f, 1f)]
    public float resourceEdgeFalloff = 1f; 

    // ===========================
    // QUESTS
    // ===========================
    [Header("Квесты")]
    public QuestEntry[] possibleQuests;
    public int questTargetsMin = 2;
    public int questTargetsMax = 5;

    // ===========================
    // Enemy
    // ===========================
    [Header("Enemy System")]
    public EnemySpawnEntry[] enemyTable;
    public float enemyDensity = 0.0008f;
    public float enemyRespawnDelay = 20f;


    // ===========================
    // EFFECTS / SKYBOX
    // ===========================
    [Header("Эффекты окружения")]
    public GameObject[] weatherPrefabs;
    public AudioClip[] ambientSounds;

    public Material skyboxMaterial;   // ← Требуется BiomeWorldVisualController

    // ===========================
    // FOG SETTINGS
    // ===========================
    [Header("Fog Settings")]
    public bool enableFog = true;
    public FogMode fogMode = FogMode.Exponential;
    public Color fogColor = Color.white;
    public float fogDensity = 0.02f;
    public float fogLinearStart = 0f;
    public float fogLinearEnd = 200f;

    // ===========================
    // WATER
    // ===========================
    [Header("Water System")]
    public bool useWater = false;
    public WaterType waterType = WaterType.Ocean;

    public float seaLevel = 1f;
    public Material waterMaterial;
    public Material swampWaterMaterial;
    public Material lakeWaterMaterial;
    public Material oceanWaterMaterial;

    // ===========================
    // LAKES
    // ===========================
    [Header("Lakes")]
    public bool generateLakes = false;

    [Tooltip("Lake threshold relative to max height")]
    public float lakeLevel = 0.6f;

    public float lakeNoiseScale = 0.05f;

    // ===========================
    // RIVERS
    // ===========================
    [Header("Rivers")]
    public bool generateRivers = false;

    public float riverNoiseScale = 0.02f;

    [Range(0f, 0.5f)]
    public float riverWidth = 0.1f;

    public float riverDepth = 2f;
}
