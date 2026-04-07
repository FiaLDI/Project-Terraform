using System.Collections.Generic;
using Features.Buffs.Domain;
using Features.Effects.Domain;
using UnityEngine;

static class EffectContextPool
{
    private static readonly Stack<EffectContext> pool = new();

    public static EffectContext Get(
        IBuffSource source,
        IBuffTarget[] targets,
        Vector3 origin,
        Vector3 direction)
    {
        if (pool.Count > 0)
        {
            var ctx = pool.Pop();
            ctx.Reset(source, targets, origin, direction);
            return ctx;
        }

        return new EffectContext(source, targets, origin, direction);
    }

    public static void Release(EffectContext ctx)
    {
        ctx.Clear(); // обнуляем ссылки
        pool.Push(ctx);
    }
}
