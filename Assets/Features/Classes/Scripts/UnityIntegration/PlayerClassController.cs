using UnityEngine;
using Features.Classes.Data;
using Features.Classes.Application;
using Features.Passives.UnityIntegration;
using Features.Abilities.Application;
using Features.Stats.UnityIntegration;
using Features.Stats.Domain;
using Features.Stats.Adapter;

[DefaultExecutionOrder(-40)]
public class PlayerClassController : MonoBehaviour
{
    [Header("Class Library")]
    public PlayerClassLibrarySO library;

    [Header("Default Class ID")]
    public string defaultClassId;

    private PlayerClassService _service;

    // ===== DOMAIN =====
    private IStatsFacade _stats;

    // ===== ADAPTERS (MonoBehaviours) =====
    private HealthStatsAdapter _health;
    private EnergyStatsAdapter _energy;
    private CombatStatsAdapter _combat;
    private MovementStatsAdapter _movement;
    private MiningStatsAdapter _mining;

    private PassiveSystem _passiveSystem;
    private AbilityCaster _abilityCaster;
    private StatBuffReceiver _buffReceiver;


    // ============================================================
    // INIT
    // ============================================================
    private void Awake()
    {
        // Создаём доменный фасад статов
        _stats = new StatsFacade();

        CacheComponents();

        // Правильно создаём и инициализируем бафф-ресивер
        _buffReceiver = gameObject.AddComponent<StatBuffReceiver>();
        _buffReceiver.Init(_stats);       // <<=== FIX: главный момент

        // Инициализируем сервис классов
        _service = new PlayerClassService(library.classes, defaultClassId);
    }


    private void Start()
    {
        if (_service.Current != null)
            ApplyClass(_service.Current);
    }


    private void CacheComponents()
    {
        _health = GetComponent<HealthStatsAdapter>();
        _energy = GetComponent<EnergyStatsAdapter>();
        _combat = GetComponent<CombatStatsAdapter>();
        _movement = GetComponent<MovementStatsAdapter>();
        _mining = GetComponent<MiningStatsAdapter>();

        _passiveSystem = GetComponent<PassiveSystem>();
        _abilityCaster = GetComponent<AbilityCaster>();
    }


    // ============================================================
    // APPLY CLASS BY ID
    // ============================================================
    public void ApplyClass(string id)
    {
        var cfg = _service.FindById(id);
        if (cfg != null)
            ApplyClass(cfg);
    }


    // ============================================================
    // APPLY CLASS CONFIG
    // ============================================================
    public void ApplyClass(PlayerClassConfigSO cfg)
    {
        if (cfg == null) return;

        _service.SelectClass(cfg);

        // ====== 1) APPLY BASE STATS (домен) ======
        _stats.Combat.ApplyBase(cfg.baseDamageMultiplier);
        _stats.Energy.ApplyBase(cfg.baseMaxEnergy, cfg.baseEnergyRegen);
        _stats.Health.ApplyBase(cfg.baseHp);

        _stats.Movement.ApplyBase(
            cfg.baseMoveSpeed,
            4f,                  // walkSpeed — дефолт
            cfg.sprintSpeed,
            cfg.crouchSpeed
        );

        _stats.Mining.ApplyBase(cfg.miningMultiplier);

        // ====== 2) INIT ADAPTERS ======
        _health?.Init(_stats.Health);
        _energy?.Init(_stats.Energy);
        _combat?.Init(_stats.Combat);
        _movement?.Init(_stats.Movement);
        _mining?.Init(_stats.Mining);

        // ====== 3) PASSIVES ======
        _passiveSystem?.SetPassives(cfg.passives.ToArray());

        // ====== 4) ABILITIES ======
        _abilityCaster?.SetAbilities(cfg.abilities.ToArray());
    }
}
