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
}
