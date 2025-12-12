using System.Collections.Generic;

public class ItemRuntimeStats
{
    private Dictionary<ItemStatType, float> stats = new();

    public float this[ItemStatType type]
    {
        get => stats.TryGetValue(type, out float v) ? v : 0;
        set => stats[type] = value;
    }
}
