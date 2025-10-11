using UnityEngine;   // <-- using всегда в начале файла

[CreateAssetMenu(menuName = "Game/Biome Config")]
public class BiomeConfig : ScriptableObject
{
    [Header("Основное")]
    public string biomeName;
    public Color mapColor;

    [Header("Размер карты")]
    public int width = 100;
    public int height = 100;

    [Header("Ландшафт")]
    public Material groundMaterial;
    public float terrainScale = 10f;
    public float heightMultiplier = 5f;

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
