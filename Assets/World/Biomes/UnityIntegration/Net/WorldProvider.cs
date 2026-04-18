using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class WorldProvider : NetworkBehaviour
{
    public readonly SyncVar<int> Seed = new();
    public readonly SyncVar<string> WorldConfigId = new();
    public readonly SyncVar<bool> HasBootstrap = new();
    public readonly SyncVar<bool> IsWorldReady = new();

    public override void OnStartServer()
    {
        base.OnStartServer();

        var session = ServerWorldSession.ConsumeBootstrap();

        Seed.Value = session.seed;
        WorldConfigId.Value = session.worldConfigId;
        HasBootstrap.Value = true;
        IsWorldReady.Value = false;
    }

    [Server]
    public void SetWorldReady()
    {
        Debug.Log("[WorldProvider] SetWorldReady called");
        IsWorldReady.Value = true;
    }
}
