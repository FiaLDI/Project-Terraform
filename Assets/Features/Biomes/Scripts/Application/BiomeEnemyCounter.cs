using UnityEngine;
using System.Collections.Generic;
using Features.Biomes.Domain;
using Features.Enemies;

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

        if (!list.Contains(inst))
            list.Add(inst);
    }

    public static void Unregister(BiomeConfig biome, EnemyInstanceTracker inst)
    {
        if (biome == null || inst == null) return;

        if (map.TryGetValue(biome, out var list))
            list.Remove(inst);
    }

    public static int GetCount(BiomeConfig biome)
    {
        if (!map.TryGetValue(biome, out var list))
            return 0;

        return list.Count;
    }

    public static int GetCountSafe(BiomeConfig biome)
    {
        if (biome == null) return 0;
        try { return GetCount(biome); }
        catch { return 0; }
    }
}