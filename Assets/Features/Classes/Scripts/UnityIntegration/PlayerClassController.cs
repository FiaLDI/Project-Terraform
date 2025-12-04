using UnityEngine;
using Features.Classes.Data;
using Features.Classes.Application;
using Features.Passives.UnityIntegration;
using Features.Abilities.Application;
using Features.Stats.UnityIntegration;
using Features.Stats.Domain;
using Features.Stats.Adapter;

[DefaultExecutionOrder(-100)]
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
    private void OnEnable()
    {
        PlayerStats.OnStatsReady += HandleStatsReady;
    }

    private void OnDisable()
    {
        PlayerStats.OnStatsReady -= HandleStatsReady;
    }


    // ============================================================
    private void Awake()
    {
        // 1) Кэшируем компоненты адаптеров
        CacheAdapters();

        // 2) Кэш пассивов и абилок
        _passiveSystem = GetComponent<PassiveSystem>();
        _abilityCaster = GetComponent<AbilityCaster>();

        // 3) Сервис классов
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


    // ============================================================
    private void HandleStatsReady(PlayerStats ps)
    {
         Debug.Log($"[STATS READY] From object: {ps.gameObject.name} | tag={ps.gameObject.tag}");
        _stats = ps.Facade;

        // Инициализация адаптеров
        _health?.Init(_stats.Health);
        _energy?.Init(_stats.Energy);
        _combat?.Init(_stats.Combat);
        _movement?.Init(_stats.Movement);
        _mining?.Init(_stats.Mining);

        // Бафф-ресивер теперь можно создавать
        if (_buffReceiver == null)
            _buffReceiver = gameObject.AddComponent<StatBuffReceiver>();

        _buffReceiver.Init(_stats);

        // Применить дефолтный класс
        if (_service.Current != null)
            ApplyClass(_service.Current);
    }


    // ============================================================
    private void Start()
    {
        // Start больше НИЧЕГО не делает — всё через HandleStatsReady()
    }


    // ============================================================
    public void ApplyClass(string id)
    {
        var cfg = _service.FindById(id);
        if (cfg != null)
            ApplyClass(cfg);
    }


    // ============================================================
    public void ApplyClass(PlayerClassConfigSO cfg)
    {
        if (cfg == null) return;

        _service.SelectClass(cfg);

        // --- APPLY DOMAIN STATS ---
        _stats.Combat.ApplyBase(cfg.baseDamageMultiplier);
        _stats.Energy.ApplyBase(cfg.baseMaxEnergy, cfg.baseEnergyRegen);
        _stats.Health.ApplyBase(cfg.baseHp);

        _stats.Movement.ApplyBase(
            cfg.baseMoveSpeed,
            4f,
            cfg.sprintSpeed,
            cfg.crouchSpeed
        );

        _stats.Mining.ApplyBase(cfg.miningMultiplier);

        // --- PASSIVES ---
        _passiveSystem?.SetPassives(cfg.passives.ToArray());

        // --- ABILITIES ---
        _abilityCaster?.SetAbilities(cfg.abilities.ToArray());
    }
}
