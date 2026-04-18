using System.Collections.Generic;
using UnityEngine;
using Features.Items.Domain;
using Features.Items.Data;
using Features.Effects.Domain;
using Features.Buffs.Domain;
using Features.Items.UnityIntegration;

public sealed class EquipmentRuntime
{
    private readonly IBuffSource source;

    private readonly Dictionary<(ItemInstance, ItemActionType), ItemRuntimeContext> runtimes
        = new();

    public EquipmentRuntime(IBuffSource source)
    {
        this.source = source;
    }

    // =====================================================
    // GET OR CREATE RUNTIME
    // =====================================================

    public ItemRuntimeContext GetRuntime(
        ItemInstance instance,
        ItemActionType actionType,
        ItemRuntimeHolder holder,
        Transform overrideMuzzle = null)
    {
        if (instance == null)
            return null;

        var key = (instance, actionType);

        if (runtimes.TryGetValue(key, out var existing))
            return existing;

        var item = instance.itemDefinition;

        if (item.actions == null)
            return null;

        foreach (var action in item.actions)
        {
            if (action.actionType == actionType)
            {
                var runtime = new ItemRuntimeContext(
                    source,
                    action
                );

                runtimes[key] = runtime;
                return runtime;
            }
        }

        return null;
    }

    // =====================================================
    // REMOVE RUNTIME
    // =====================================================

    public void Remove(ItemInstance instance)
    {
        if (instance == null)
            return;

        var keysToRemove = new List<(ItemInstance, ItemActionType)>();

        foreach (var kv in runtimes)
        {
            if (kv.Key.Item1 == instance)
            {
                kv.Value.StopUse();
                keysToRemove.Add(kv.Key);
            }
        }

        foreach (var key in keysToRemove)
            runtimes.Remove(key);
    }

    // =====================================================
    // CLEAR ALL
    // =====================================================

    public void Clear()
    {
        foreach (var runtime in runtimes.Values)
            runtime.StopUse();

        runtimes.Clear();
    }
}