using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryRateLimitMiddleware : IInventoryCommandMiddleware
{
    private readonly Dictionary<int, float> lastTime = new();

    public float cooldown = 0.05f;

    public void Execute(InventoryCommandContext ctx, Action next)
    {
        int clientId = ctx.Sender.ClientId;

        if (lastTime.TryGetValue(clientId, out var last))
        {
            if (Time.time - last < cooldown)
            {
                Debug.LogWarning("[Middleware] Spam blocked");
                return;
            }
        }

        lastTime[clientId] = Time.time;

        next();
    }
}
