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

    /* ================= PUBLIC STATE ================= */

    public string CurrentClassId =>
        currentClass != null ? currentClass.id : defaultClassId;

    public string CurrentVisualId =>
        currentClass != null && currentClass.visualPreset != null
            ? currentClass.visualPreset.id
            : null;

    /* ================= LIFECYCLE ================= */

    private void Awake()
    {
        visualController = GetComponent<PlayerVisualController>();
        passiveSystem   = GetComponent<PassiveSystem>();
        abilityCaster   = GetComponent<AbilityCaster>();
        buffTarget      = GetComponent<PlayerBuffTarget>();

        classService = new PlayerClassService(
            library.classes,
            defaultClassId
        );
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
        stats = ps.Facade;
        buffTarget?.SetStats(stats);

        // если класс уже был назначен ранее — применяем
        if (currentClass != null)
            StartCoroutine(ApplyDeferred(currentClass));
    }

    /* ================= API ================= */

    /// <summary>
    /// Единственный публичный способ применить класс
    /// </summary>
    public void ApplyClass(string classId)
    {
        var cfg = library.FindById(classId);

        if (cfg == null)
        {
            Debug.LogWarning(
                $"[PlayerClassController] Class {classId} not found, using default",
                this
            );
            cfg = library.FindById(defaultClassId);
        }

        currentClass = cfg;

        if (stats != null)
            StartCoroutine(ApplyDeferred(cfg));
    }

    /* ================= APPLY ================= */

    private IEnumerator ApplyDeferred(PlayerClassConfigSO cfg)
    {
        yield return null;
        ApplyInternal(cfg);
    }

    private void ApplyInternal(PlayerClassConfigSO cfg)
    {
        if (cfg == null || stats == null)
            return;

        Debug.Log($"[PlayerClassController] Applying class: {cfg.displayName}", this);

        classService.SelectClass(cfg);

        var p = cfg.preset;

        // ===== BASE STATS =====

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

        // ===== PASSIVES / ABILITIES =====

        passiveSystem?.SetPassives(cfg.passives.ToArray());
        abilityCaster?.SetAbilities(cfg.abilities.ToArray());

        // ===== VISUAL =====

        if (cfg.visualPreset != null)
            visualController.ApplyVisual(cfg.visualPreset.id);
    }
}
