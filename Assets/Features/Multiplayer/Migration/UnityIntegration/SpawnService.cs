using UnityEngine;
using FishNet;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Managing;
using Multiplayer.Domain;
using Multiplayer.Application;

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

        flow.OnStateChanged += OnServerStateChanged;

        if (PlayerSpawnRegistry.I != null)
            PlayerSpawnRegistry.I.OnProviderRegistered += TrySpawnAllPending;
    }

    public void HandleLoginSpawn(NetworkConnection conn, PlayerSession session)
    {
        if (!CanSpawnNow())
            return;

        SpawnInternal(conn, session);
    }

    private void OnServerStateChanged(ServerGameState state)
    {
        if (state != ServerGameState.Running)
            return;

        TrySpawnAllPending();
    }

    private void TrySpawnAllPending()
    {
        if (!CanSpawnNow())
            return;

        foreach (var session in sessionManager.GetOnlineSessions())
        {
            if (!session.IsOnline)
                continue;

            if (session.PlayerObject != null)
                continue;

            if (!nm.ServerManager.Clients.TryGetValue(session.ClientId.Value, out var conn))
                continue;

            SpawnInternal(conn, session);
        }
    }

    private bool CanSpawnNow()
    {
        Debug.Log($"CanSpawnNow? State={flow.CurrentState} " +
              $"RegistryNull={PlayerSpawnRegistry.I == null} " +
              $"HasProvider={(PlayerSpawnRegistry.I != null && PlayerSpawnRegistry.I.HasProvider)}");

        if (flow.CurrentState != ServerGameState.Running)
            return false;

        if (PlayerSpawnRegistry.I == null)
            return false;

        if (!PlayerSpawnRegistry.I.HasProvider)
            return false;

        return true;
    }

    private void SpawnInternal(NetworkConnection conn, PlayerSession session)
{
    if (!PlayerSpawnRegistry.I.TryGetRandom(out var provider))
    {
        Debug.LogWarning("No spawn providers available.");
        return;
    }

    if (!provider.TryGetSpawnPoint(out var pos, out var rot))
        return;

    var playerObj = Object.Instantiate(playerPrefab, pos, rot);

    nm.ServerManager.Spawn(playerObj, conn);

    session.SetPlayerObject(playerObj);

    Debug.Log($"Spawned NEW player for {conn.ClientId}");

    NotifyClientSpawned(conn);
}




    private void NotifyClientSpawned(NetworkConnection conn)
    {
        foreach (var obj in conn.Objects)
        {
            if (obj.TryGetComponent<ClientLoginBridge>(out var bridge))
            {
                bridge.NotifySpawnedTargetRpc(conn);
                break;
            }
        }
    }

    public void RespawnAllOnline()
    {
        foreach (var session in sessionManager.GetOnlineSessions())
        {
            if (!session.IsOnline)
                continue;

            if (!nm.ServerManager.Clients.TryGetValue(session.ClientId.Value, out var conn))
                continue;

            void Handler(NetworkConnection c, bool asServer)
            {
                // отписываемся сразу
                conn.OnLoadedStartScenes -= Handler;

                if (session.PlayerObject != null)
                {
                    nm.ServerManager.Despawn(session.PlayerObject);
                    session.SetPlayerObject(null);
                }

                SpawnInternal(conn, session);
            }

            conn.OnLoadedStartScenes += Handler;
        }
    }

}
