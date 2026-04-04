using System;
using UnityEngine;

public class InventoryLoggingMiddleware : IInventoryCommandMiddleware
{
    public void Execute(InventoryCommandContext ctx, Action next)
    {
        Debug.Log($"[CMD] {ctx.Command.Command} from {ctx.Sender.ClientId}");
        next();
    }
}
