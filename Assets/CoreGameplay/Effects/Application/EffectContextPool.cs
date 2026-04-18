using System;
using System.Collections.Generic;
using Features.Buffs.Domain;
using Features.Effects.Domain;
using UnityEngine;

static class EffectContextPool
{
    // 🔥 пул по типам
    private static readonly Dictionary<Type, Stack<EffectContext>> pools = new();

    // ======================================================
    // BASE
    // ======================================================

    public static EffectContext Get(
        IBuffSource source,
        IBuffTarget[] targets,
        Vector3 origin,
        Vector3 direction)
    {
        return Get<EffectContext>(source, targets, origin, direction);
    }

    // ======================================================
    // GENERIC
    // ======================================================

    public static T Get<T>(
        IBuffSource source,
        IBuffTarget[] targets,
        Vector3 origin,
        Vector3 direction)
        where T : EffectContext, new()
    {
        var type = typeof(T);

        if (!pools.TryGetValue(type, out var stack))
        {
            stack = new Stack<EffectContext>();
            pools[type] = stack;
        }

        T ctx;

        if (stack.Count > 0)
        {
            ctx = (T)stack.Pop();
            ctx.Reset(source, targets, origin, direction);
        }
        else
        {
            ctx = new T();
            ctx.Reset(source, targets, origin, direction);
        }

        return ctx;
    }

    // ======================================================
    // RELEASE
    // ======================================================

    public static void Release(EffectContext ctx)
    {
        if (ctx == null)
            return;

        var type = ctx.GetType();

        if (!pools.TryGetValue(type, out var stack))
        {
            stack = new Stack<EffectContext>();
            pools[type] = stack;
        }

        ctx.Clear();
        stack.Push(ctx);
    }
}
