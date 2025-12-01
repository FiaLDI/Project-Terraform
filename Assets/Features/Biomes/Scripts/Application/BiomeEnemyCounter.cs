using UnityEngine;
using System.Collections.Generic;
using Features.Biomes.Domain;
using Features.Enemies;

public static class EnemyBiomeCounter
{
    private static readonly Dictionary<BiomeConfig, List<EnemyDefinition>> map =
        new();

    public static void Register(BiomeConfig biome, EnemyDefinition def)
    {
        if (biome == null || def == null) return;

        if (!map.TryGetValue(biome, out var list))
        {
            list = new List<EnemyDefinition>();
            map[biome] = list;
        }

        if (!list.Contains(def))
            list.Add(def);
    }

    public static void Unregister(BiomeConfig biome, EnemyDefinition def)
    {
        if (biome == null || def == null) return;

        if (map.TryGetValue(biome, out var list))
            list.Remove(def);
    }

    public static int GetCount(BiomeConfig biome)
    {
        if (!map.TryGetValue(biome, out var list))
            return 0;

        return list.Count;
    }
}


public class EnemyAutoUnregister : MonoBehaviour
{
    public BiomeConfig biome;
    public EnemyDefinition definition;

    void OnDisable()
    {
        if (definition != null)
            EnemyWorldManager.Instance?.Unregister(definition);

        if (biome != null && definition != null)
            EnemyBiomeCounter.Unregister(biome, definition);
    }

    void OnDestroy()
    {
        if (definition != null)
            EnemyWorldManager.Instance?.Unregister(definition);

        if (biome != null && definition != null)
            EnemyBiomeCounter.Unregister(biome, definition);
    }
}
