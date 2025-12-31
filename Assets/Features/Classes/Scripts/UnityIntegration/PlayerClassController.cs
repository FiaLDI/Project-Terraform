using UnityEngine;
using Features.Classes.Data;
using Features.Classes.Application;
using Features.Passives.UnityIntegration;
using Features.Abilities.Application;
using Features.Stats.Domain;
using Features.Buffs.UnityIntegration;
using Features.Stats.UnityIntegration;

[RequireComponent(typeof(PlayerVisualController))]
public sealed class PlayerClassController : MonoBehaviour
{
    // =====================================================
    // CONFIG
    // =====================================================

    [Header("Classes Library")]
    [SerializeField] private PlayerClassLibrarySO library;

    [Header("Default Class ID")]
    [SerializeField] private string defaultClassId = "engineer";

    // =====================================================
    // COMPONENTS
    // =====================================================

    private PassiveSystem passiveSystem;
    private AbilityCaster abilityCaster;
    private PlayerBuffTarget buffTarget;

    // =====================================================
    // DOMAIN
    // =====================================================

    private PlayerClassService classService;
    private IStatsFacade stats;
    private PlayerClassConfigSO currentClass;

    public PlayerClassConfigSO GetCurrentClass() => currentClass;

    public string CurrentClassId =>
        currentClass != null ? currentClass.id : defaultClassId;

    public string CurrentVisualId =>
        currentClass != null && currentClass.visualPreset != null
            ? currentClass.visualPreset.id
            : null;

    /// <summary>
    /// Вызывается, когда класс полностью применён
    /// (пассивки, способности, регистрация бафов).
    /// </summary>
    public event System.Action OnClassApplied;

    // =====================================================
    // LIFECYCLE
    // =====================================================

    private void Awake()
    {
        passiveSystem = GetComponent<PassiveSystem>();
        abilityCaster = GetComponent<AbilityCaster>();
        buffTarget = GetComponent<PlayerBuffTarget>();

        if (library == null)
        {
            Debug.LogError("[PlayerClassController] PlayerClassLibrarySO not assigned!", this);
            enabled = false;
            return;
        }

        classService = new PlayerClassService(
            library.classes,
            defaultClassId
        );

        Debug.Log("[PlayerClassController] Initialized", this);
    }

    private void OnEnable()
    {
        PlayerStats.OnStatsReady += OnStatsReady;
    }

    private void OnDisable()
    {
        PlayerStats.OnStatsReady -= OnStatsReady;
    }

    // =====================================================
    // STATS BINDING
    // =====================================================

    private void OnStatsReady(PlayerStats ps)
    {
        stats = ps.Facade;
        buffTarget?.SetStats(stats);

        Debug.Log("[PlayerClassController] Stats bound", this);
    }

    // =====================================================
    // PUBLIC API (SERVER ONLY)
    // =====================================================

    /// <summary>
    /// Применить класс.
    /// Вызывается ТОЛЬКО сервером из PlayerStateNetAdapter,
    /// ТОЛЬКО когда PlayerStats уже готов.
    /// </summary>
    public void ApplyClass(string classId)
    {
        if (stats == null)
        {
            Debug.LogError(
                "[PlayerClassController] ApplyClass called before stats ready!",
                this
            );
            return;
        }

        var cfg = library.FindById(classId);
        if (cfg == null)
        {
            Debug.LogWarning(
                $"[PlayerClassController] Class '{classId}' not found, using default '{defaultClassId}'",
                this
            );
            cfg = library.FindById(defaultClassId);
        }

        ApplyInternal(cfg);
    }

    // =====================================================
    // INTERNAL APPLY
    // =====================================================

    private void ApplyInternal(PlayerClassConfigSO cfg)
    {
        if (cfg == null)
        {
            Debug.LogError("[PlayerClassController] ApplyInternal called with null config", this);
            return;
        }

        currentClass = cfg;

        Debug.Log($"[PlayerClassController] Applying class '{cfg.displayName}'", this);

        // 1️⃣ Доменные данные (выбранный класс)
        classService.SelectClass(cfg);

        // 2️⃣ Пассивки (регистрируют бафы, но НЕ меняют статы напрямую)
        passiveSystem?.SetPassives(cfg.passives.ToArray());

        // 3️⃣ Способности (регистрация, без прямых статов)
        abilityCaster?.SetAbilities(cfg.abilities.ToArray());

        // 4️⃣ Событие завершения
        OnClassApplied?.Invoke();

        Debug.Log($"[PlayerClassController] ✅ Class '{cfg.displayName}' applied", this);
    }
}
