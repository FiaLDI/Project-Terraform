
using System.Collections.Generic;
using Features.Inventory.Domain;
using UnityEngine;

public class InventoryCommandRouter
{
    private readonly Dictionary<InventoryCommand, InventoryCommandPipeline> pipelines = new();

    public void Register(InventoryCommand cmd, InventoryCommandPipeline pipeline)
    {
        pipelines[cmd] = pipeline;
    }

    public void Execute(InventoryCommandContext ctx)
    {
        if (!pipelines.TryGetValue(ctx.Command.Command, out var pipeline))
        {
            Debug.LogWarning($"[Router] No pipeline for {ctx.Command.Command}");
            return;
        }

        pipeline.Execute(ctx);
    }
}
