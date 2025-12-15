using UnityEngine;
using System.Collections;
using Features.Classes.Data;
using Features.Classes.Application;
using Features.Passives.UnityIntegration;
using Features.Abilities.Application;
using Features.Stats.UnityIntegration;
using Features.Stats.Domain;
using Features.Stats.Adapter;
using Features.Buffs.UnityIntegration;

[DefaultExecutionOrder(-100)]
public class PlayerClassController : MonoBehaviour
{
    [Header("Classes Library")]
    public PlayerClassLibrarySO library;

    [Header("Default Class ID (fallback for tests)")]
    public string defaultClassId = "engineer";

    [Header("Visuals")]
    private PlayerVisualController visualController;

    private PlayerClassService _classService;
    private PlayerClassConfigSO _pendingClassCfg;

    // DOMAIN facade
    private IStatsFacade _stats;

    // Adapters
    private HealthStatsAdapter _health;
    private EnergyStatsAdapter _energy;
    private CombatStatsAdapter _combat;
    private MovementStatsAdapter _movement;
    private MiningStatsAdapter _mining;

    private PassiveSystem _passiveSystem;
    private AbilityCaster _abilityCaster;
    private PlayerBuffTarget _buffTarget;

    private void OnEnable()
    {
        PlayerStats.OnStatsReady += HandleStatsReady;
    }

    private void OnDisable()
    {
        PlayerStats.OnStatsReady -= HandleStatsReady;
    }

    private void Awake()
    {
        CacheAdapters();

        _buffTarget = GetComponent<PlayerBuffTarget>();
        _passiveSystem = GetComponent<PassiveSystem>();
        _abilityCaster = GetComponent<AbilityCaster>();

        if (visualController == null)
            visualController = GetComponent<PlayerVisualController>();

        _classService = new PlayerClassService(library.classes, defaultClassId);

        SelectInitialClass();
    }

    private void CacheAdapters()
    {
        _health = GetComponent<HealthStatsAdapter>();
        _energy = GetComponent<EnergyStatsAdapter>();
        _combat = GetComponent<CombatStatsAdapter>();
        _movement = GetComponent<MovementStatsAdapter>();
        _mining = GetComponent<MiningStatsAdapter>();
    }

    private void SelectInitialClass()
    {
        var progress = PlayerProgressService.Instance;
        var activeChar = progress != null ? progress.GetActiveCharacter() : null;

        if (activeChar != null)
        {
            _pendingClassCfg = library.FindById(activeChar.classId);

            if (_pendingClassCfg != null)
            {
                Debug.Log("[PlayerClass] Using character class: " + activeChar.classId);
                return;
            }
        }

        // fallback
        _pendingClassCfg = library.FindById(defaultClassId);
        Debug.Log("[PlayerClass] Using DEFAULT class: " + defaultClassId);
    }

    private void HandleStatsReady(PlayerStats ps)
    {
        _stats = ps.Facade;

        _health?.Init(_stats.Health);
        _energy?.Init(_stats.Energy);
        _combat?.Init(_stats.Combat);
        _movement?.Init(_stats.Movement);
        _mining?.Init(_stats.Mining);

        _buffTarget?.SetStats(_stats);

        if (_pendingClassCfg != null)
            StartCoroutine(DelayedApplyClass(_pendingClassCfg));
    }

    private IEnumerator DelayedApplyClass(PlayerClassConfigSO cfg)
    {
        yield return null;

        yield return new WaitUntil(() =>
            _abilityCaster != null &&
            _abilityCaster.Energy != null
        );

        ApplyClassInternal(cfg);
    }

    private void ApplyClassInternal(PlayerClassConfigSO cfg)
    {
        if (cfg == null) return;

        Debug.Log($"[PlayerClass] Applying: {cfg.displayName}");

        _classService.SelectClass(cfg);

        var p = cfg.preset;

        _stats.Health.ApplyBase(p.health.baseHp);
        _stats.Health.ApplyRegenBase(p.health.baseRegen);

        _stats.Energy.ApplyBase(p.energy.baseMaxEnergy, p.energy.baseRegen);

        _stats.Combat.ApplyBase(p.combat.baseDamageMultiplier);

        _stats.Movement.ApplyBase(
            p.movement.baseSpeed,
            p.movement.walkSpeed,
            p.movement.sprintSpeed,
            p.movement.crouchSpeed,
            p.movement.rotationSpeed
        );

        _stats.Mining.ApplyBase(p.mining.baseMining);

        _passiveSystem?.SetPassives(cfg.passives.ToArray());
        _abilityCaster?.SetAbilities(cfg.abilities.ToArray());


        if (cfg.visualPreset != null)
        {
            Debug.Log("SKIN");
            Debug.Log(cfg.visualPreset.id);
            visualController?.ApplyVisual(cfg.visualPreset.id);
        }
        else
        {
            Debug.LogWarning($"[PlayerClass] Class {cfg.displayName} has no visual preset");
        }
    }
}
