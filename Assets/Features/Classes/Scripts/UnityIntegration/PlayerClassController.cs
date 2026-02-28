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

        string safeDefault =
            library.classes.Count > 0
                ? library.classes[0].id
                : null;

        classService = new PlayerClassService(
            library.classes,
            safeDefault
        );
    }

    // =====================================================
    // SERVER API
    // =====================================================

    public void ApplyClass(string classId)
    {
       Debug.Log($"[CLASS] Requested classId = {classId}");

        var cfg = library.FindById(classId);

        if (cfg == null)
        {
            string safeDefault =
                library.classes.Count > 0
                    ? library.classes[0].id
                    : null;
            cfg = library.FindById(safeDefault);
        }

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
