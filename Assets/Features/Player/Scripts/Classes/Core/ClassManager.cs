using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(PlayerEnergy))]
public class ClassManager : MonoBehaviour
{
    [SerializeField] private PlayerClass startingClass;

    public PlayerClass CurrentClass { get; private set; }
    public AbilitySO[] ActiveAbilities { get; private set; } = new AbilitySO[5];

    private PlayerEnergy _energy;
    private BuffSystem _buffSystem;

    private readonly List<PassiveSO> appliedPassives = new();

    private void Awake()
    {
        _energy = GetComponent<PlayerEnergy>();
        _buffSystem = GetComponent<BuffSystem>();

        if (startingClass != null)
            ApplyClass(startingClass);
    }

    public void ApplyClass(PlayerClass pc)
    {
        if (pc == null)
        {
            Debug.LogError("ClassManager: CLASS IS NULL!");
            return;
        }

        CurrentClass = pc;

        // =====================================================
        // CAST TO SPECIFIC CLASS TYPE
        // (т.к. разные классы могут иметь разные наборы свойств)
        // =====================================================
        if (pc is EngineerTechnicianSO tech)
        {
            ApplyTechnician(tech);
        }
        else
        {
            Debug.LogWarning($"ClassManager: Unsupported class type {pc.GetType().Name}");
        }
    }

    private void ApplyTechnician(EngineerTechnicianSO cls)
    {
        // 1️⃣ Энергия
        _energy.SetMaxEnergy(cls.baseEnergy, true);
        _energy.SetRegen(cls.regen);

        // 2️⃣ Активные способности
        for (int i = 0; i < 5; i++)
        {
            ActiveAbilities[i] =
                (i < cls.activeAbilities.Count ? cls.activeAbilities[i] : null);
        }

        // 3️⃣ Пассивки
        ApplyPassives(cls.passiveBonuses);
    }

    private void ApplyPassives(List<PassiveSO> passives)
    {
        // удаляем предыдущие
        foreach (var p in appliedPassives)
            p.Remove(gameObject);

        appliedPassives.Clear();

        // добавляем новые
        foreach (var p in passives)
        {
            p.Apply(gameObject);
            appliedPassives.Add(p);
        }
    }
}
