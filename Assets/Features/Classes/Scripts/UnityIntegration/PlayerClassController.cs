using UnityEngine;
using System.Collections;
using Features.Classes.Data;
using Features.Classes.Application;
using Features.Passives.UnityIntegration;
using Features.Abilities.Application;
using Features.Stats.UnityIntegration;
using Features.Stats.Domain;
using Features.Buffs.UnityIntegration;


[RequireComponent(typeof(PlayerVisualController))]
public sealed class PlayerClassController : MonoBehaviour
{
    [Header("Classes Library")]
    [SerializeField] private PlayerClassLibrarySO library;

    [Header("Default Class ID")]
    [SerializeField] private string defaultClassId = "engineer";

    /* ================= COMPONENTS ================= */

    private PlayerVisualController visualController;
    private PassiveSystem passiveSystem;
    private AbilityCaster abilityCaster;
    private PlayerBuffTarget buffTarget;

    /* ================= DOMAIN ================= */

    private PlayerClassService classService;
    private IStatsFacade stats;


    private PlayerClassConfigSO currentClass;
    public PlayerClassConfigSO GetCurrentClass() => currentClass;


    /* ================= PUBLIC STATE ================= */


    public string CurrentClassId =>
        currentClass != null ? currentClass.id : defaultClassId;


    public string CurrentVisualId =>
        currentClass != null && currentClass.visualPreset != null
            ? currentClass.visualPreset.id
            : null;


    // 🟢 EVENT: когда класс полностью применён (с баффами)
    public event System.Action OnClassApplied;


    /* ================= LIFECYCLE ================= */


    private void Awake()
    {
        visualController = GetComponent<PlayerVisualController>();
        passiveSystem = GetComponent<PassiveSystem>();
        abilityCaster = GetComponent<AbilityCaster>();
        buffTarget = GetComponent<PlayerBuffTarget>();

        if (visualController == null)
            Debug.LogError("[PlayerClassController] PlayerVisualController not found!", this);
        if (passiveSystem == null)
            Debug.LogError("[PlayerClassController] PassiveSystem not found!", this);
        if (abilityCaster == null)
            Debug.LogError("[PlayerClassController] AbilityCaster not found!", this);
        if (buffTarget == null)
            Debug.LogError("[PlayerClassController] PlayerBuffTarget not found!", this);

        if (library == null)
        {
            Debug.LogError("[PlayerClassController] PlayerClassLibrarySO not assigned!", this);
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

    /* ================= STATS ================= */

    private void OnStatsReady(PlayerStats ps)
    {
        Debug.Log("[PlayerClassController] Stats ready event received", this);

        stats = ps.Facade;
        if (stats == null)
        {
            Debug.LogError("[PlayerClassController] Stats facade is null!", this);
            return;
        }

        buffTarget?.SetStats(stats);

        if (currentClass != null)
        {
            Debug.Log($"[PlayerClassController] Stats ready, applying queued class: {currentClass.id}", this);
            StartCoroutine(ApplyDeferred(currentClass));
        }
        else
        {
            Debug.Log("[PlayerClassController] Stats ready, no class queued yet", this);
        }
    }

    /* ================= PUBLIC API ================= */

    /// <summary>
    /// 🟢 Применить класс по ID
    /// Может быть вызван до инициализации статов - класс будет применен когда статы будут готовы
    /// </summary>
    public void ApplyClass(string classId)
    {
        if (string.IsNullOrEmpty(classId))
        {
            Debug.LogWarning("[PlayerClassController] ApplyClass called with empty classId", this);
            return;
        }

        Debug.Log($"[PlayerClassController] ApplyClass called: {classId}", this);

        var cfg = library.FindById(classId);

        if (cfg == null)
        {
            Debug.LogWarning(
                $"[PlayerClassController] Class '{classId}' not found in library, using default '{defaultClassId}'",
                this
            );
            cfg = library.FindById(defaultClassId);
        }

        currentClass = cfg;

        if (stats != null)
        {
            Debug.Log($"[PlayerClassController] Stats ready, applying class immediately: {classId}", this);
            StartCoroutine(ApplyDeferred(cfg));
        }
        else
        {
            Debug.Log($"[PlayerClassController] Stats not ready yet, queuing class: {classId}", this);
        }
    }

    /* ================= APPLY ================= */


    /// <summary>
    /// Отложенное применение класса (на следующий фрейм)
    /// Позволяет синхронизировать применение с другими системами
    /// </summary>
    private IEnumerator ApplyDeferred(PlayerClassConfigSO cfg)
    {
        yield return null;
        ApplyInternal(cfg);
    }

    /// <summary>
    /// 🟢 Основной метод применения класса
    /// Применяет статы, пассивки, абилити и визуал
    /// </summary>
    private void ApplyInternal(PlayerClassConfigSO cfg)
    {
        if (cfg == null)
        {
            Debug.LogError("[PlayerClassController] ApplyInternal called with null config", this);
            return;
        }

        if (stats == null)
        {
            Debug.LogError("[PlayerClassController] ApplyInternal called but stats is null", this);
            return;
        }

        Debug.Log($"[PlayerClassController] Applying class: {cfg.displayName}", this);

        classService.SelectClass(cfg);

        var p = cfg.preset;

        Debug.Log($"[PlayerClassController] Applying base stats for {cfg.displayName}", this);
        stats.Health.ApplyBase(p.health.baseHp);
        stats.Health.ApplyRegenBase(p.health.baseRegen);

        stats.Energy.ApplyBase(
            p.energy.baseMaxEnergy,
            p.energy.baseRegen
        );

        stats.Combat.ApplyBase(
            p.combat.baseDamageMultiplier
        );

        stats.Movement.ApplyBase(
            p.movement.baseSpeed,
            p.movement.walkSpeed,
            p.movement.sprintSpeed,
            p.movement.crouchSpeed,
            p.movement.rotationSpeed
        );

        stats.Mining.ApplyBase(
            p.mining.baseMining
        );


        Debug.Log($"[PlayerClassController] Applying passives and abilities", this);
        
        passiveSystem?.SetPassives(cfg.passives.ToArray());
        
        abilityCaster?.SetAbilities(cfg.abilities.ToArray());

        OnClassApplied?.Invoke();

        Debug.Log($"[PlayerClassController] ✅ Class applied completely: {cfg.displayName}", this);
    }
}
