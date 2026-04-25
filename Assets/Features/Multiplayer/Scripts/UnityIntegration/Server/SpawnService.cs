using UnityEngine;
using FishNet;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Managing;
using Multiplayer.Domain;
using Multiplayer.Application;
using Features.Class.Net;
using Features.Stats.UnityIntegration;
using Features.Player.UnityIntegration;
using System.Linq;

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
            var netObj = session.PlayerObject;

            if (netObj.Owner != null)
                netObj.RemoveOwnership();

            netObj.GiveOwnership(conn);
            session.BindClient(conn.ClientId);

            return;
        }

        if (!CanSpawnNow())
            return;

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

    public void RespawnSession(PlayerSession session)
    {
        if (session == null || !session.IsOnline || !session.ClientId.HasValue)
            return;

        if (!CanSpawnNow())
        {
            Debug.Log("[fix-net] RespawnSession aborted: world not ready");
            return;
        }

        if (!nm.ServerManager.Clients.TryGetValue(session.ClientId.Value, out var conn))
            return;

        if (session.PlayerObject != null)
        {
            nm.ServerManager.Despawn(session.PlayerObject);
            session.SetPlayerObject(null);
        }

        SpawnInternal(conn, session);
    }


    // =============================
    // INTERNAL SPAWN
    // =============================
   private NetworkObject SpawnInternal(NetworkConnection conn, PlayerSession session)
    {
        if (!PlayerSpawnRegistry.I.TryGetSpawnPoint(conn.ClientId, out var pos, out var rot))
            return null;

        var playerObj = Object.Instantiate(playerPrefab, pos, rot);
        var state = playerObj.GetComponent<PlayerStateNetwork>();
        state.PreInit(session.ClassId, session.Level, session.PassiveIds.ToArray());

        nm.ServerManager.Spawn(playerObj, conn);

        session.SetPlayerObject(playerObj);

        Debug.Log($"[fix-net] Spawned player for conn={conn.ClientId}");

        return playerObj;
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
