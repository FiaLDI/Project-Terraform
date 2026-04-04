using System.Collections.Generic;

public class InventoryCommandPipeline
{
    private readonly List<IInventoryCommandMiddleware> middlewares = new();

    public void Add(IInventoryCommandMiddleware middleware)
    {
        middlewares.Add(middleware);
    }

    public void Execute(InventoryCommandContext ctx)
    {
        int index = -1;

        void Next()
        {
            index++;

            if (index < middlewares.Count)
                middlewares[index].Execute(ctx, Next);
        }

        Next();
    }
}
