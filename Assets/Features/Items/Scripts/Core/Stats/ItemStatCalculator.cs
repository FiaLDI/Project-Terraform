public static class ItemStatCalculator
{
    public static ItemRuntimeStats Calculate(Item item)
    {
        var result = new ItemRuntimeStats();

        if (item.baseStats != null)
            foreach (var s in item.baseStats)
                result[s.stat] = s.value;

        if (item.upgrades != null)
        {
            for (int i = 0; i <= item.currentLevel && i < item.upgrades.Length; i++)
                foreach (var s in item.upgrades[i].bonusStats)
                    result[s.stat] += s.value;
        }

        foreach (ItemStatType type in System.Enum.GetValues(typeof(ItemStatType)))
        {
            float original = result[type];
            float modified = GlobalStatManager.instance.Apply(type, original);
            result[type] = modified;
        }

        return result;
    }

}
