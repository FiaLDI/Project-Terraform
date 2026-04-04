using System;

public class InventoryCommandHandlerMiddleware : IInventoryCommandMiddleware
{
    private readonly Action<InventoryCommandContext> handler;

    public InventoryCommandHandlerMiddleware(Action<InventoryCommandContext> handler)
    {
        this.handler = handler;
    }

    public void Execute(InventoryCommandContext ctx, Action next)
    {
        handler(ctx);
    }
}
