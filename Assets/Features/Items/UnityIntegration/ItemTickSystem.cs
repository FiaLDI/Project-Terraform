using System.Collections.Generic;
using FishNet.Object;
using FishNet.Managing.Timing;
using UnityEngine;

public sealed class ItemTickSystem : NetworkBehaviour
{
    private static ItemTickSystem instance;

    private readonly HashSet<IItemTickable> items = new();

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

        foreach (var item in items)
            item.ServerTick(dt);
    }
}