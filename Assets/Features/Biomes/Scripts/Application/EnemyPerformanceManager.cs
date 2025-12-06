using UnityEngine;

/// <summary>
/// Управляет глобальными параметрами производительности:
/// - динамически подстраивает дистанции LOD у всех мобов
/// - регулирует общий лимит мобов в мире (EnemyCountScale)
/// Все работает в реальном времени и очень стабильно.
/// </summary>
[DefaultExecutionOrder(-200)]
public class EnemyPerformanceManager : MonoBehaviour
{
    public static EnemyPerformanceManager Instance { get; private set; }

    [Header("Target Performance (FPS)")]
    public float targetFPS = 60f;
    public float minFPS = 30f;    // ниже этого → жёстко режем LOD

    [Header("LOD Scaling")]
    public float minLODScale = 0.5f; // ближе включаем LOD2/instancing
    public float maxLODScale = 1.5f; // можем позволить себе дальние LOD

    [Header("Enemy Count Scaling")]
    public float minEnemyScale = 0.5f; 
    public float maxEnemyScale = 1.0f;

    [Header("Stability Settings")]
    [Tooltip("Насколько быстро система реагирует на изменение FPS (0–1).")]
    public float responsiveness = 0.1f;

    [Tooltip("Максимальное изменение LodScale за 1 кадр (стабилизирует рывки).")]
    public float maxLODStep = 0.05f;

    [Tooltip("Максимальное изменение EnemyCountScale за 1 кадр.")]
    public float maxEnemyStep = 0.05f;

    public float LodScale { get; private set; } = 1f;
    public float EnemyCountScale { get; private set; } = 1f;

    private float smoothedFPS;

    private void Awake()
    {
        if (Instance && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        smoothedFPS = targetFPS;
    }

    private void Update()
    {
        float rawFPS = 1f / Mathf.Max(Time.unscaledDeltaTime, 0.0001f);

        // --- Экспоненциальное сглаживание FPS ---
        smoothedFPS = Mathf.Lerp(smoothedFPS, rawFPS, responsiveness);

        // --- Нормализуем FPS в диапазон 0..1 ---
        float t = Mathf.InverseLerp(minFPS, targetFPS, smoothedFPS);

        // --- Целевые значения ---
        float targetLOD = Mathf.Lerp(minLODScale, maxLODScale, t);
        float targetEnemy = Mathf.Lerp(minEnemyScale, maxEnemyScale, t);

        // --- Плавный переход с ограничением скорости ---
        LodScale = SmoothStep(LodScale, targetLOD, maxLODStep);
        EnemyCountScale = SmoothStep(EnemyCountScale, targetEnemy, maxEnemyStep);
    }

    private float SmoothStep(float current, float target, float maxStep)
    {
        float delta = target - current;
        float step = Mathf.Clamp(delta, -maxStep, maxStep);
        return current + step;
    }
}
