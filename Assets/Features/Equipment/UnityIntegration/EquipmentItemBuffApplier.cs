using Features.Buffs.Application;
using Features.Buffs.Domain;
using Features.Items.Domain;
using UnityEngine;

public sealed class EquipmentItemBuffApplier : MonoBehaviour, IBuffSource
{
    private BuffSystem buffSystem;

    private void Awake()
    {
        buffSystem = GetComponent<BuffSystem>();
    }

    public void Apply(ItemInstance inst)
    {
        if (inst == null || inst.IsEmpty)
            return;

        var def = inst.itemDefinition;
        if (def == null)
            return;

        if (def.equippedBuffs != null)
        {
            foreach (var buff in def.equippedBuffs)
            {
                if (buff != null)
                    buffSystem.Add(buff, this);
            }
        }

        if (def.upgrades != null &&
            inst.level >= 0 &&
            inst.level < def.upgrades.Length)
        {
            var upgrade = def.upgrades[inst.level];

            if (upgrade?.levelBuffs != null)
            {
                foreach (var buff in upgrade.levelBuffs)
                {
                    if (buff != null)
                        buffSystem.Add(buff, this);
                }
            }
        }
    }

    public void Remove()
    {
        if (buffSystem != null)
            buffSystem.RemoveBySource(this);
    }
}
