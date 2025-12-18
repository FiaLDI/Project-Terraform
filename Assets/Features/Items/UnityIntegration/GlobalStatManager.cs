using System.Collections.Generic;
using UnityEngine;

public class GlobalStatManager : MonoBehaviour
{
    public static GlobalStatManager instance;

    // Список всех активных глобальных бафов
    private List<GlobalStatModifier> modifiers = new();

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public void AddModifier(GlobalStatModifier mod)
    {
        modifiers.Add(mod);
    }

    public void RemoveModifier(GlobalStatModifier mod)
    {
        modifiers.Remove(mod);
    }

    public float Apply(ItemStatType stat, float baseValue)
    {
        float result = baseValue;

        foreach (var mod in modifiers)
        {
            if (mod.stat != stat)
                continue;

            // 1) Flat
            result += mod.flatBonus;

            // 2) Percent
            result *= (1f + mod.percentBonus);
        }

        return result;
    }
}
