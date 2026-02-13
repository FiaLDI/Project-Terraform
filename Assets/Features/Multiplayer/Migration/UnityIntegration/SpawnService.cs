using UnityEngine;
using FishNet;
using FishNet.Connection;
using FishNet.Object;
using Multiplayer.Domain;
using Multiplayer.Application;
using FishNet.Managing;

public sealed class SpawnService
{
    private readonly NetworkManager nm;
    private readonly SessionManager sessionManager;
    private readonly IServerGameFlow flow;
    private readonly NetworkObject playerPrefab;

    public SpawnService(
        NetworkManager nm,
        SessionManager sessionManager,
        IServerGameFlow flow,
        NetworkObject prefab)
    {
        this.nm = nm;
        this.sessionManager = sessionManager;
        this.flow = flow;
        this.playerPrefab = prefab;
    }

    public void HandleLoginSpawn(NetworkConnection conn, PlayerSession session)
    {
        if (flow.CurrentState != ServerGameState.Running)
            return;

        if (session.PlayerObject != null)
        {
            nm.ServerManager.Spawn(session.PlayerObject, conn);
            return;
        }

        if (!PlayerSpawnRegistry.I.TryGetRandom(out var provider))
            return;

        if (!provider.TryGetSpawnPoint(out var pos, out var rot))
            return;

        var player = Object.Instantiate(playerPrefab, pos, rot);
        nm.ServerManager.Spawn(player, conn);

        session.SetPlayerObject(player);
    }
}
