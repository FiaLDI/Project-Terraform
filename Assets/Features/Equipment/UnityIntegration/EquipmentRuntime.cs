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

    private readonly Dictionary<ItemInstance, ItemRuntimeContext> runtimes
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

        if (runtimes.TryGetValue(instance, out var existing))
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

                runtimes[instance] = runtime;
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

        if (!runtimes.TryGetValue(instance, out var runtime))
            return;

        runtime.StopUse();

        runtimes.Remove(instance);
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