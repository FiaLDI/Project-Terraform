using UnityEngine;
using Features.Classes.Data;
using Features.Classes.Application;
using Features.Passives.UnityIntegration;
using Features.Abilities.Application;
using Features.Abilities.Domain;
using Features.Passives.Net;
using Features.Buffs.Application;

[RequireComponent(typeof(PlayerVisualController))]
public sealed class PlayerClassController : MonoBehaviour
{
    [Header("Classes Library")]
    [SerializeField] private PlayerClassLibrarySO library;

    private PassiveSystem passiveSystem;
    private AbilityCaster abilityCaster;
    private BuffSystem buffSystem;
    private ServerGamePhase phase;

    private PlayerClassService classService;
    private PlayerClassConfigSO currentClass;
    public PlayerClassConfigSO currentClassOut => currentClass;

    public event System.Action OnClassApplied;

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

        if (cfg == null)
        {
            Debug.LogError("[PlayerClassController] No valid class config to apply", this);
            return;
        }

        currentClass = cfg;
        classService.SelectClass(cfg);
        abilityCaster.SetAbilities(
            cfg.abilities != null
                ? cfg.abilities.ToArray()
                : System.Array.Empty<AbilitySO>()
        );

        if (phase.IsAtLeast(GamePhase.BuffsReady))
        {
            ApplyPassives();
        }
        else
        {
            phase.OnPhaseReached -= OnPhaseReached;
            phase.OnPhaseReached += OnPhaseReached;
        }
    }

    private void OnPhaseReached(GamePhase p)
    {
        if (p == GamePhase.BuffsReady)
            ApplyPassives();
    }

    private void ApplyPassives()
    {
        phase.OnPhaseReached -= OnPhaseReached;

        phase.Reach(GamePhase.PassivesApplied);

        OnClassApplied?.Invoke();
    }

    private void OnDestroy()
    {
        if (phase != null)
            phase.OnPhaseReached -= OnPhaseReached;
    }
}
