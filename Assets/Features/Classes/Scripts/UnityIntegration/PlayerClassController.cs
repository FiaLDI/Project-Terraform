using UnityEngine;
using Features.Classes.Data;
using Features.Classes.Application;
using Features.Passives.UnityIntegration;
using Features.Abilities.Application;
using Features.Passives.Net;
using Features.Buffs.Application;

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
    private BuffSystem buffSystem;
    private ServerGamePhase phase;

    // =====================================================
    // DOMAIN
    // =====================================================

    private PlayerClassService classService;
    private PlayerClassConfigSO currentClass;

    public event System.Action OnClassApplied;

    // =====================================================
    // LIFECYCLE
    // =====================================================

    private void Awake()
    {
        passiveSystem = GetComponent<PassiveSystem>();
        abilityCaster = GetComponent<AbilityCaster>();
        buffSystem = GetComponent<BuffSystem>();
        phase = GetComponent<ServerGamePhase>();

        if (library == null)
        {
            Debug.LogError("[PlayerClassController] Class library missing", this);
            enabled = false;
            return;
        }

        classService = new PlayerClassService(
            library.classes,
            defaultClassId
        );
    }

    // =====================================================
    // SERVER API
    // =====================================================

    public void ApplyClass(string classId)
    {
        var cfg = library.FindById(classId)
            ?? library.FindById(defaultClassId);

        currentClass = cfg;

        classService.SelectClass(cfg);
        abilityCaster.SetAbilities(cfg.abilities.ToArray());

        if (phase.IsAtLeast(GamePhase.BuffsReady))
        {
            ApplyPassives();
        }
        else
        {
            phase.OnPhaseReached += OnPhaseReached;
        }
    }

    // =====================================================
    // PHASE
    // =====================================================

    private void OnPhaseReached(GamePhase p)
    {
        if (p == GamePhase.BuffsReady)
            ApplyPassives();
    }

    private void ApplyPassives()
    {
        phase.OnPhaseReached -= OnPhaseReached;

        Debug.Log("[PASSIVES] Apply", this);

        var net = GetComponent<PassiveNetAdapter>();
        net.ServerSetPassives(currentClass.passives.ToArray());

        phase.Reach(GamePhase.PassivesApplied);

        OnClassApplied?.Invoke();
    }
}
