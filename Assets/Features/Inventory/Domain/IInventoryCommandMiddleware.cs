using System;

public interface IInventoryCommandMiddleware
{
    void Execute(InventoryCommandContext context, Action next);
}
