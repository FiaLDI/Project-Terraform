using System.Collections.Generic;

namespace Features.Tools.Domain
{
    public enum ToolStat
    {
        MiningSpeed,
        Damage,
        Range,
        ScanRange,
        ScanSpeed,
        Cooldown,
        ThrowForce
    }

    public class ToolRuntimeStats
    {
        private readonly Dictionary<ToolStat, float> values = new();

        public float this[ToolStat stat]
        {
            get => values.TryGetValue(stat, out float v) ? v : 0f;
            set => values[stat] = value;
        }

        public void Add(ToolStat stat, float value)
        {
            if (!values.ContainsKey(stat))
                values[stat] = 0;

            values[stat] += value;
        }
    }
}
