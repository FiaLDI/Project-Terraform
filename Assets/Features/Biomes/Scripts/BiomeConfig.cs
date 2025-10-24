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
    [Header("Квест и префаб")]
    public QuestAsset questAsset;          // какой квест
    public GameObject questPointPrefab;    // какой префаб для его целей

    [Header("Спавн")]
    [Range(0f, 1f)] 
    public float spawnChance = 1f;         // шанс появления (1 = всегда, 0.5 = 50%)

    public int minTargets = 1;             // минимум целей для этого квеста
    public int maxTargets = 3;             // максимум целей для этого квеста
}

[System.Serializable]
public class EnvironmentEntry
{
    [Header("Префаб окружения")]
    public GameObject prefab;

    [Header("Шанс спавна (0–1)")]
    [Range(0f, 1f)] public float spawnChance = 1f;

    [Header("Вес (вероятность выбора типа)")]
    [Range(0f, 10f)] public float weight = 1f;
}

[CreateAssetMenu(menuName = "Game/Biome Config")]
public class BiomeConfig : ScriptableObject
{
    
    [Header("Основное")]
    public string biomeName;
    public Color mapColor;

    [Header("Генерация")]
    public bool isGenerate = true;

    [Header("Размер карты")]
    public int width = 100;
    public int height = 100;

    [Header("Рельеф")]
    public TerrainType terrainType = TerrainType.SmoothHills;
    public Material groundMaterial;
    public float terrainScale = 10f;
    public float heightMultiplier = 5f;

    [Header("Fractal Mountains (только для FractalMountains)")]
    [Range(1, 8)] public int fractalOctaves = 5;
    [Range(0.1f, 1f)] public float fractalPersistence = 0.5f;
    [Range(1.5f, 4f)] public float fractalLacunarity = 2f;

    [Header("Окружение")]
    public EnvironmentEntry[] environmentPrefabs;
    [Range(0f, 1f)] public float environmentDensity = 0.05f;


    [Header("Ресурсы")]
    public GameObject[] resourcePrefabs;
    [Range(0f, 1f)] public float resourceDensity = 0.02f;

    [Header("Квесты")]
    public QuestEntry[] possibleQuests; // список (квест + префаб)
    public int questTargetsMin = 2;
    public int questTargetsMax = 5;

    [Header("Эффекты окружения")]
    public GameObject[] weatherPrefabs;
    public AudioClip[] ambientSounds;
    public Material skyboxMaterial;

    [Header("Fog Settings")]
    public bool enableFog = true;
    public FogMode fogMode = FogMode.Exponential;
    public Color fogColor = new Color(0.9f, 0.45f, 0.2f);
    public float fogDensity = 0.02f;
    public float fogLinearStart = 0f;
    public float fogLinearEnd = 200f;

}
