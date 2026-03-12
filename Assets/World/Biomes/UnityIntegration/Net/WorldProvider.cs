using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class WorldProvider : NetworkBehaviour
{
    public readonly SyncVar<int> Seed = new();
    public readonly SyncVar<bool> IsWorldReady = new();

    public override void OnStartServer()
    {
        base.OnStartServer();

        int seed = ServerWorldSession.PendingSeed;
        Debug.Log("SERVER SESSION QUESTS: " + ServerWorldSession.PendingQuestIds.Count);

        Debug.Log("INITIALIZE SEED: " + seed);

        Seed.Value = seed;
        IsWorldReady.Value = false;
    }

    [Server]
    public void SetWorldReady()
    {
         Debug.Log("[WorldProvider] SetWorldReady called");
        IsWorldReady.Value = true;
    }
}