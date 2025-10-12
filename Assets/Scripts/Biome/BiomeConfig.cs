using UnityEngine;

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

[CreateAssetMenu(menuName = "Game/Biome Config")]
public class BiomeConfig : ScriptableObject
{
    [Header("Основное")]
    public string biomeName;
    public Color mapColor;

    [Header("Размер карты")]
    public int width = 100;
    public int height = 100;

    [Header("Рельеф")]
    public TerrainType terrainType = TerrainType.SmoothHills;
    public Material groundMaterial;
    public float terrainScale = 10f;
    public float heightMultiplier = 5f;

    [Header("Fractal Mountains (только если выбран этот тип)")]
    [Range(1, 8)] public int fractalOctaves = 5;      // количество слоёв шума
    [Range(0.1f, 1f)] public float fractalPersistence = 0.5f; // уменьшение амплитуды
    [Range(1.5f, 4f)] public float fractalLacunarity = 2f;    // увеличение частоты

    [Header("Объекты окружения")]
    public GameObject[] environmentPrefabs;
    [Range(0f, 1f)] public float environmentDensity = 0.05f;

    [Header("Ресурсы")]
    public GameObject[] resourcePrefabs;
    [Range(0f, 1f)] public float resourceDensity = 0.02f;

    [Header("Квестовые точки")]
    public GameObject[] questPrefabs;
    [Range(0f, 1f)] public float questSpawnChance = 0.01f;

    [Header("Эффекты")]
    public GameObject[] weatherPrefabs;
    public AudioClip[] ambientSounds;

    [Header("Небо")]
    public Material skyboxMaterial;
}
