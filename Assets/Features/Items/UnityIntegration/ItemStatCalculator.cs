using Features.Items.Domain;
using Features.Items.Data;
using UnityEngine;

public static class ItemStatCalculator
{
    public static ItemRuntimeStats Calculate(ItemInstance inst)
    {
        var result = new ItemRuntimeStats();
        var def = inst.itemDefinition;

        // =====================================================
        // BASE STATS (from item definition)
        // =====================================================
        if (def.baseStats != null)
        {
            foreach (var s in def.baseStats)
            {
                result[s.stat] += s.value;
            }
        }

        // =====================================================
        // UPGRADES (level-based)
        // =====================================================
        if (def.upgrades != null && inst.level > 0)
        {
            int max = Mathf.Min(inst.level, def.upgrades.Length);

            for (int i = 0; i < max; i++)
            {
                var upgrade = def.upgrades[i];
                if (upgrade?.bonusStats == null)
                    continue;

                foreach (var s in upgrade.bonusStats)
                {
                    result[s.stat] += s.value;
                }
            }
        }

        return result;
    }
}
