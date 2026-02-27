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
    }

    // =============================
    // LOGIN SPAWN (первый вход)
    // =============================
    public void HandleLoginSpawn(NetworkConnection conn, PlayerSession session)
{
        if (session.PlayerObject != null)
        {
            Debug.Log($"[fix-net] Login spawn skipped, already has player for {conn.ClientId}");
            return;
        }

        if (!CanSpawnNow())
        {
            Debug.Log($"[fix-net] World not ready, spawn deferred for {conn.ClientId}");
            return;
        }

        SpawnInternal(conn, session);
    }


    // =============================
    // SCENE RESPAWN (смена сцены)
    // =============================
    public void RespawnAllOnline()
    {
        Debug.Log("[SpawnService] RespawnAllOnline called");

        if (!CanSpawnNow())
        {
            Debug.Log("[fix-net] Respawn aborted: world not ready");
            return;
        }

        foreach (var session in sessionManager.GetOnlineSessions())
        {
            if (!nm.ServerManager.Clients.TryGetValue(session.ClientId.Value, out var conn))
                continue;

            if (session.PlayerObject == null)
            {
                Debug.Log($"[fix-net] First spawn for {conn.ClientId}");
                SpawnInternal(conn, session);
                continue;
            }

            Debug.Log($"[fix-net] Respawning existing player for {conn.ClientId}");

            nm.ServerManager.Despawn(session.PlayerObject);
            session.SetPlayerObject(null);

            SpawnInternal(conn, session);
        }

        Debug.Log("[fix-net] === RespawnAllOnline END ===");
    }


    // =============================
    // INTERNAL SPAWN
    // =============================
    private void SpawnInternal(NetworkConnection conn, PlayerSession session)
    {
        if (!PlayerSpawnRegistry.I.TryGetRandom(out var provider))
        {
            Debug.LogWarning("[fix-net] No spawn providers available.");
            return;
        }

        if (!provider.TryGetSpawnPoint(out var pos, out var rot))
        {
            Debug.LogWarning("[fix-net] Spawn point unavailable.");
            return;
        }

        var playerObj = Object.Instantiate(playerPrefab, pos, rot);

        nm.ServerManager.Spawn(playerObj, conn);

        session.SetPlayerObject(playerObj);

        Debug.Log($"[fix-net] Spawned player for conn={conn.ClientId}");

        if (playerObj.TryGetComponent<ClientLoginBridge>(out var bridge))
        {
            bridge.NotifySpawnedTargetRpc(conn);
            Debug.Log($"[fix-net] Direct TargetRpc sent to {conn.ClientId}");
        }
        else
        {
            Debug.LogError("[fix-net] ClientLoginBridge NOT FOUND on player prefab!");
        }
    }

    // =============================
    // UTIL
    // =============================
    private bool CanSpawnNow()
    {
        if (flow.CurrentState != ServerGameState.Running)
            return false;

        if (PlayerSpawnRegistry.I == null)
            return false;

        if (!PlayerSpawnRegistry.I.HasProvider)
            return false;

        return true;
    }

}
