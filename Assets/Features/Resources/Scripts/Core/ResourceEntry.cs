using UnityEngine;

public enum ResourceClusterType
{
    Single,
    CrystalVein,        // плотная жила, врезается в рельеф
    RoundCluster,       // куча в круге
    VerticalStackNoise  // цилиндр/столб по noise
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

    [Tooltip("Чем выше значение — тем сильнее карта влияет на шанс спавна")]
    [Range(0f, 2f)]
    public float distributionStrength = 1f;

    [Tooltip("Шкала шума — регулирует размер кластеров")]
    public float distributionScale = 0.05f;

    [Tooltip("Сдвиг карты — чтобы разные ресурсы не совпадали")]
    public Vector2 distributionOffset;


    [Header("Biome Rules")]
    public TerrainType[] allowedBiomes;
}
