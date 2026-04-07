using System.Collections.Generic;
using Features.Effects.Application;
using Features.Effects.Domain;
using UnityEngine;

static class EffectCache
{
    private static readonly Dictionary<EffectDefinition, IEffect> cache = new();

    public static IEffect Get(EffectDefinition def)
    {
        if (!IsCacheable(def))
            return EffectFactory.Create(def);

        if (cache.TryGetValue(def, out var e))
            return e;

        e = EffectFactory.Create(def);
        if (e == null)
        {
            Debug.LogError($"[EFFECT EXECUTOR] Effect NULL: {def.type}");
        }
        cache[def] = e;
        
        return e;
    }

    private static bool IsCacheable(EffectDefinition def)
    {
        return def.type switch
        {
            EffectType.HealInstant => true,
            EffectType.ApplyBuff => true,
            EffectType.RemoveBuffSource => true,
            EffectType.Scan => true,

            _ => false
        };
    }
}