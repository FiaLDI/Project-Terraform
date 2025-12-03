using UnityEngine;
using Features.Classes.Data;
using Features.Classes.Application;
using Features.Passives.UnityIntegration;

[DefaultExecutionOrder(-40)]
public class PlayerClassController : MonoBehaviour
{
    [Header("Class Library")]
    public PlayerClassLibrarySO library;

    [Header("Default Class ID")]
    public string defaultClassId;

    private PlayerClassService _service;

    private PlayerHealth _health;
    private PlayerEnergy _energy;
    private PlayerCombat _combat;
    private PlayerMovementStats _movement;
    private PlayerMiningStats _mining;
    private PassiveSystem _passiveSystem;
    private AbilityCaster _abilityCaster;

    private void Start()
    {
        if (_service.Current != null)
            ApplyClass(_service.Current);
    }

    private void Awake()
    {
        CacheComponents();

        _service = new PlayerClassService(library.classes, defaultClassId);

    }

    private void CacheComponents()
    {
        _health = GetComponent<PlayerHealth>();
        _energy = GetComponent<PlayerEnergy>();
        _combat = GetComponent<PlayerCombat>();
        _movement = GetComponent<PlayerMovementStats>();
        _mining = GetComponent<PlayerMiningStats>();
        _passiveSystem = GetComponent<PassiveSystem>();
        _abilityCaster = GetComponent<AbilityCaster>();
    }

    public void ApplyClass(string id)
    {
        var cfg = _service.FindById(id);
        if (cfg != null)
            ApplyClass(cfg);
    }

    public void ApplyClass(PlayerClassConfigSO cfg)
    {
        if (cfg == null) return;

        _service.SelectClass(cfg);

        // HEALTH
        _health?.SetMaxHp(cfg.baseHp);
        _health?.SetShield(cfg.baseShield);

        // ENERGY
        _energy?.SetMaxEnergy(cfg.baseMaxEnergy, true);
        _energy?.SetRegen(cfg.baseEnergyRegen);

        // COMBAT
        _combat?.SetBaseDamage(cfg.baseDamageMultiplier);

        // MOVEMENT
        if (_movement != null)
        {
            _movement.SetBaseSpeed(cfg.baseMoveSpeed);
            _movement.SetSprint(cfg.sprintSpeed);
            _movement.SetCrouch(cfg.crouchSpeed);
        }

        // MINING
        _mining?.SetMultiplier(cfg.miningMultiplier);

        // PASSIVES
        if (_passiveSystem != null)
            _passiveSystem.SetPassives(cfg.passives.ToArray());

        // ABILITIES
        if (_abilityCaster != null)
            _abilityCaster.SetAbilities(cfg.abilities.ToArray());
    }
}
