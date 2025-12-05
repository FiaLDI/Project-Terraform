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

    [Header("Default Class ID")]
    public string defaultClassId;

    private PlayerClassService _service;

    // DOMAIN facade
    private IStatsFacade _stats;

    // ADAPTERS
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
        if (_buffTarget == null)
            Debug.LogError("[PlayerClassController] PlayerBuffTarget missing!");

        _passiveSystem = GetComponent<PassiveSystem>();
        _abilityCaster = GetComponent<AbilityCaster>();

        _service = new PlayerClassService(library.classes, defaultClassId);
    }

    private void CacheAdapters()
    {
        _health = GetComponent<HealthStatsAdapter>();
        _energy = GetComponent<EnergyStatsAdapter>();
        _combat = GetComponent<CombatStatsAdapter>();
        _movement = GetComponent<MovementStatsAdapter>();
        _mining = GetComponent<MiningStatsAdapter>();
    }

    private void HandleStatsReady(PlayerStats ps)
    {
        Debug.Log($"[STATS READY] From: {ps.gameObject.name}");

        _stats = ps.Facade;

        // Init Adapters
        _health?.Init(_stats.Health);
        _energy?.Init(_stats.Energy);
        _combat?.Init(_stats.Combat);
        _movement?.Init(_stats.Movement);
        _mining?.Init(_stats.Mining);

        // BUFF TARGET
        _buffTarget?.SetStats(_stats);

        // apply default class (but delayed)
        if (_service.Current != null)
            StartCoroutine(DelayedApplyClass(_service.Current));
    }

    private IEnumerator DelayedApplyClass(PlayerClassConfigSO cfg)
    {
        yield return null;

        // Wait until AbilityCaster is fully initialized
        yield return new WaitUntil(() =>
            _abilityCaster != null &&
            _abilityCaster.Energy != null
        );

        ApplyClassInternal(cfg);
    }

    public void ApplyClass(string id)
    {
        var cfg = _service.FindById(id);
        if (cfg != null)
            StartCoroutine(DelayedApplyClass(cfg));
    }

    public void ApplyClass(PlayerClassConfigSO cfg)
    {
        if (cfg != null)
            StartCoroutine(DelayedApplyClass(cfg));
    }

    private void ApplyClassInternal(PlayerClassConfigSO cfg)
    {
        if (cfg == null) return;

        Debug.Log($"[Class] Applying class: {cfg.displayName}");

        _service.SelectClass(cfg);

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
    }
}
