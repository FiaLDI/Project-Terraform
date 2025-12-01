using UnityEngine;

[DefaultExecutionOrder(-200)]
public class EnemyPerformanceManager : MonoBehaviour
{
    public static EnemyPerformanceManager Instance { get; private set; }

    [Header("Target Performance")]
    public float targetFPS = 60f;
    public float minFPS = 30f;

    [Header("LOD Scaling")]
    public float minLODScale = 0.5f; // ближе включаем LOD2/instancing
    public float maxLODScale = 1.5f; // можем позволить себе дальние LOD0/1

    [Header("Enemy Count Scaling")]
    public float minEnemyScale = 0.5f;
    public float maxEnemyScale = 1.0f;

    public float LodScale { get; private set; } = 1f;
    public float EnemyCountScale { get; private set; } = 1f;

    private float fpsSmoothed;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        float currentFPS = 1f / Mathf.Max(Time.unscaledDeltaTime, 0.0001f);
        fpsSmoothed = Mathf.Lerp(fpsSmoothed, currentFPS, 0.1f);

        float t = Mathf.InverseLerp(minFPS, targetFPS, fpsSmoothed);

        LodScale = Mathf.Lerp(minLODScale, maxLODScale, t);
        EnemyCountScale = Mathf.Lerp(minEnemyScale, maxEnemyScale, t);
    }
}
