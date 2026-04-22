using System.Collections.Generic;
using FishNet.Object;
using FishNet.Managing.Timing;
using UnityEngine;

public sealed class ItemTickSystem : NetworkBehaviour
{
    private static ItemTickSystem instance;

    private readonly HashSet<IItemTickable> items = new();
    private readonly List<IItemTickable> tickBuffer = new();

    public static void Register(IItemTickable item)
    {
        if (instance != null)
            instance.items.Add(item);
    }

    public static void Unregister(IItemTickable item)
    {
        if (instance != null)
            instance.items.Remove(item);
    }

    public override void OnStartServer()
    {
        instance = this;
        TimeManager.OnTick += OnServerTick;
    }

    public override void OnStopServer()
    {
        TimeManager.OnTick -= OnServerTick;
        items.Clear();
        instance = null;
    }

    private void OnServerTick()
    {
        float dt = (float)TimeManager.TickDelta;

        tickBuffer.Clear();
        tickBuffer.AddRange(items);

        foreach (var item in tickBuffer)
        {
            if (!items.Contains(item))
                continue;

            item.ServerTick(dt);
        }

        tickBuffer.Clear();
    }
}
