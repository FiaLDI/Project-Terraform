using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(PlayerEnergy))]
public class ClassManager : MonoBehaviour
{
    [SerializeField] private PlayerClass startingClass;

    public PlayerClass CurrentClass { get; private set; }
    public AbilitySO[] ActiveAbilities { get; private set; } = new AbilitySO[5];
    public System.Action OnAbilitiesChanged;

    private PlayerEnergy _energy;
    private BuffSystem _buffSystem;
    private readonly List<PassiveSO> appliedPassives = new();

    private void Awake()
    {
        _energy = GetComponent<PlayerEnergy>();
        _buffSystem = GetComponent<BuffSystem>();
    }

    private void Start()
    {
        if (startingClass != null)
            ApplyClass(startingClass);
        
        
        Debug.Log($"ClassManager: ActiveAbilities count = {ActiveAbilities.Length}");
        for (int i = 0; i < ActiveAbilities.Length; i++)
        {
            Debug.Log($"Slot {i}: {(ActiveAbilities[i] != null ? ActiveAbilities[i].name : "NULL")}");
        }
    }

    public void ApplyClass(PlayerClass pc)
    {
        if (pc == null)
        {
            Debug.LogError("ClassManager: CLASS IS NULL!");
            return;
        }

        CurrentClass = pc;

        if (pc is EngineerTechnicianSO tech)
            ApplyTechnician(tech);
        else
            Debug.LogWarning($"ClassManager: Unsupported class type {pc.GetType().Name}");
    }

    private void ApplyTechnician(EngineerTechnicianSO cls)
    {
        _energy.SetMaxEnergy(cls.baseEnergy, true);
        _energy.SetRegen(cls.regen);

        for (int i = 0; i < 5; i++)
            ActiveAbilities[i] = (i < cls.activeAbilities.Count ? cls.activeAbilities[i] : null);

        ApplyPassives(cls.passiveBonuses);

        OnAbilitiesChanged?.Invoke();
    }

    private void ApplyPassives(List<PassiveSO> passives)
    {
        foreach (var p in appliedPassives)
            p.Remove(gameObject);

        appliedPassives.Clear();

        foreach (var p in passives)
        {
            p.Apply(gameObject);
            appliedPassives.Add(p);
        }
    }
}
