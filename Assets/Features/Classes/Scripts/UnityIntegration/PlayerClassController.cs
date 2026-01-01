using UnityEngine;
using Features.Classes.Data;
using Features.Classes.Application;
using Features.Passives.UnityIntegration;
using Features.Abilities.Application;
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

    // =====================================================
    // DOMAIN
    // =====================================================

    private PlayerClassService classService;
    private PlayerClassConfigSO currentClass;

    public PlayerClassConfigSO GetCurrentClass() => currentClass;

    public string CurrentClassId =>
        currentClass != null ? currentClass.id : defaultClassId;

    public string CurrentVisualId =>
        currentClass != null && currentClass.visualPreset != null
            ? currentClass.visualPreset.id
            : null;
    private PlayerBuffTarget buffTarget;

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
            Debug.LogError(
                "[PlayerClassController] PlayerClassLibrarySO not assigned!",
                this
            );
            enabled = false;
            return;
        }

        classService = new PlayerClassService(
            library.classes,
            defaultClassId
        );

        Debug.Log("[PlayerClassController] Initialized", this);
    }

    // =====================================================
    // PUBLIC API (SERVER ONLY)
    // =====================================================

    /// <summary>
    /// Применить класс.
    /// Вызывается ТОЛЬКО сервером из PlayerStateNetAdapter.
    /// Предполагается, что PlayerStats уже инициализирован.
    /// </summary>
    public void ApplyClass(string classId)
    {
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
            Debug.LogError(
                "[PlayerClassController] ApplyInternal called with null config",
                this
            );
            return;
        }

        currentClass = cfg;

        Debug.Log(
            $"[PlayerClassController] Applying class '{cfg.displayName}'",
            this
        );

        // 1️⃣ Доменные данные (выбранный класс)
        classService.SelectClass(cfg);


        // 3️⃣ Способности
        abilityCaster?.SetAbilities(cfg.abilities.ToArray());
        
        
        // 2️⃣ Пассивки
        if (buffTarget != null)
        {
            buffTarget.OnReady -= ApplyPassivesSafe;
            buffTarget.OnReady += ApplyPassivesSafe;
        }

        // 4️⃣ Сигнал завершения
        OnClassApplied?.Invoke();

        Debug.Log(
            $"[PlayerClassController] ✅ Class '{cfg.displayName}' applied",
            this
        );
    }

    private void ApplyPassivesSafe()
    {
        passiveSystem?.SetPassives(currentClass.passives.ToArray());
    }
}
