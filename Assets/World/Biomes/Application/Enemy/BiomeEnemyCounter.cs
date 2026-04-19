using System.Collections.Generic;
using Biomes.Data;

namespace Biomes.Application
{
    public static class EnemyBiomeCounter
    {
        private static readonly Dictionary<BiomeConfig, List<EnemyInstanceTracker>> map =
            new();

        public static void Register(BiomeConfig biome, EnemyInstanceTracker inst)
        {
            if (biome == null || inst == null) return;

            if (!map.TryGetValue(biome, out var list))
            {
                list = new List<EnemyInstanceTracker>();
                map[biome] = list;
            }

            Cleanup(list);

            if (!list.Contains(inst))
                list.Add(inst);
        }

        public static void Unregister(BiomeConfig biome, EnemyInstanceTracker inst)
        {
            if (biome == null || inst == null) return;

            if (!map.TryGetValue(biome, out var list))
                return;

            list.Remove(inst);
            Cleanup(list);

            if (list.Count == 0)
                map.Remove(biome);
        }

        public static int GetCount(BiomeConfig biome)
        {
            if (!map.TryGetValue(biome, out var list))
                return 0;

            Cleanup(list);
            return list.Count;
        }

        public static int GetCountSafe(BiomeConfig biome)
        {
            if (biome == null) return 0;
            try { return GetCount(biome); }
            catch { return 0; }
        }

        public static void ClearAll()
        {
            map.Clear();
        }

        private static void Cleanup(List<EnemyInstanceTracker> list)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var tracker = list[i];
                if (tracker == null || !tracker.isActiveAndEnabled)
                    list.RemoveAt(i);
            }
        }
    }
}
