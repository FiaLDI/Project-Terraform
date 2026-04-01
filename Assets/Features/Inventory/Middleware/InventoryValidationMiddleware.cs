using System;
using Features.Inventory.Domain;
using UnityEngine;

public class InventoryValidationMiddleware : IInventoryCommandMiddleware
{
    public void Execute(InventoryCommandContext ctx, Action next)
    {
        if (ctx.Inventory == null || ctx.Inventory.Service == null)
        {
            Debug.LogError("[Middleware] Inventory invalid");
            return;
        }

        if (ctx.Command.Command == InventoryCommand.None)
        {
            Debug.LogError("[Middleware] Command is None");
            return;
        }

        next();
    }
}
