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

[System.Serializable]
public class QuestEntry
{
    public QuestAsset questAsset;
    public GameObject questPointPrefab;

#if UNITY_EDITOR
    [HideInInspector] public Texture2D previewTexture;
#endif

    [Range(0f, 1f)]
    public float spawnChance = 1f;

    public int minTargets = 1;
    public int maxTargets = 3;
}

[System.Serializable]
public class EnvironmentEntry
{
    public GameObject prefab;

    [Range(0f, 1f)]
    public float spawnChance = 1f;

    [Range(0f, 10f)]
    public float weight = 1f;
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

    // Размеры (используются BiomeHeightUtility)
    [Header("Размер карты (для озёр/воды)")]
    public int width = 100;
    public int height = 100;

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
    [Range(0f, 0.01f)] public float resourceDensity = 0.001f;
    public float resourceSpawnYOffset = 0.3f;

    // ===========================
    // QUESTS
    // ===========================
    [Header("Квесты")]
    public QuestEntry[] possibleQuests;
    public int questTargetsMin = 2;
    public int questTargetsMax = 5;

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
    [Header("Global Water")]
    public bool useWater = false;
    public float seaLevel = 1f;
    public Material waterMaterial;

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
