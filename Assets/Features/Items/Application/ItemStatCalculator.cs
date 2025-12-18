

using System;
using Features.Items.Data;

namespace Features.Items.Application
{
    public class ItemStatCalculator : IItemStatCalculator
    {
        private readonly IGlobalStatProvider _global;

        public ItemStatCalculator(IGlobalStatProvider global)
        {
            _global = global;
        }

        public ItemRuntimeStats Calculate(Item item, int level)
        {
            var result = new ItemRuntimeStats();

            // Base stats
            foreach (var s in item.baseStats)
                result[s.stat] = s.value;

            // Upgrades
            for (int i = 0; i <= level && i < item.upgrades.Length; i++)
            {
                foreach (var s in item.upgrades[i].bonusStats)
                    result[s.stat] += s.value;
            }

            // Global modifiers
            foreach (ItemStatType t in Enum.GetValues(typeof(ItemStatType)))
                result[t] = _global.Apply(t, result[t]);

            return result;
        }
    }
}
