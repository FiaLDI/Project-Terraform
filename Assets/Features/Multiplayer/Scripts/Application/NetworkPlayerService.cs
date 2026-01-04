using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq; // 🔥 ВАЖНО
using Features.Player.UnityIntegration;

public sealed class NetworkPlayerService : MonoBehaviour
{
    public static NetworkPlayerService I { get; private set; }

    [SerializeField] private NetworkObject playerPrefab;
    [SerializeField] private string defaultClassId = "0";

    private readonly Dictionary<int, NetworkObject> players = new();
    private readonly Dictionary<int, string> classByConn = new();
    private readonly HashSet<int> waitingForSpawn = new();

    private NetworkManager nm;

    // ==========================================================

    private void Awake()
    {
        if (I != null)
        {
            Destroy(gameObject);
            return;
        }

        I = this;
        DontDestroyOnLoad(gameObject);

        nm = InstanceFinder.NetworkManager;
    }

    private void OnEnable()
    {
        if (nm == null)
            return;

        nm.ServerManager.OnRemoteConnectionState += OnRemoteConnectionState;
        nm.SceneManager.OnClientLoadedStartScenes += OnClientLoadedStartScenes;

        if (PlayerSpawnRegistry.I != null)
        {
            PlayerSpawnRegistry.I.OnProviderRegistered += TrySpawnWaiting;
            PlayerSpawnRegistry.I.OnProviderUnregistered += TrySpawnWaiting;
        }
    }

    private void OnDisable()
    {
        if (nm == null)
            return;

        nm.ServerManager.OnRemoteConnectionState -= OnRemoteConnectionState;
        nm.SceneManager.OnClientLoadedStartScenes -= OnClientLoadedStartScenes;

        if (PlayerSpawnRegistry.I != null)
        {
            PlayerSpawnRegistry.I.OnProviderRegistered -= TrySpawnWaiting;
            PlayerSpawnRegistry.I.OnProviderUnregistered -= TrySpawnWaiting;
        }
    }

    // ==========================================================
    // SPAWN ENTRY
    // ==========================================================

    private void OnClientLoadedStartScenes(NetworkConnection conn, bool asServer)
    {
        if (asServer)
            TrySpawn(conn);
    }

    public void TrySpawn(NetworkConnection conn)
    {
        if (conn == null || players.ContainsKey(conn.ClientId))
            return;

        if (PlayerSpawnRegistry.I == null ||
            !PlayerSpawnRegistry.I.TryGetRandom(out var provider))
        {
            waitingForSpawn.Add(conn.ClientId);
            return;
        }

        SpawnPlayer(conn, provider);
    }

    private void TrySpawnWaiting()
    {
        if (!nm.IsServer || PlayerSpawnRegistry.I == null)
            return;

        foreach (int clientId in waitingForSpawn.ToArray())
        {
            if (!nm.ServerManager.Clients.TryGetValue(clientId, out var conn))
            {
                waitingForSpawn.Remove(clientId);
                continue;
            }

            if (PlayerSpawnRegistry.I.TryGetRandom(out var provider))
            {
                SpawnPlayer(conn, provider);
                waitingForSpawn.Remove(clientId);
            }
        }
    }

    // ==========================================================

    private void SpawnPlayer(NetworkConnection conn, IPlayerSpawnProvider provider)
    {
        if (!provider.TryGetSpawnPoint(out var pos, out var rot))
            return;

        var player = Instantiate(playerPrefab, pos, rot);

        if (player.TryGetComponent(out PlayerStateNetwork psn))
        {
            psn.PreInitClass(GetOrCreateClass(conn));
        }

        nm.ServerManager.Spawn(player, conn);
        players[conn.ClientId] = player;

        Debug.Log($"[PlayerService] Spawned player {conn.ClientId}");
    }

    // ==========================================================

    private void OnRemoteConnectionState(
        NetworkConnection conn,
        FishNet.Transporting.RemoteConnectionStateArgs args)
    {
        if (args.ConnectionState ==
            FishNet.Transporting.RemoteConnectionState.Stopped)
        {
            Despawn(conn);
        }
    }

    private void Despawn(NetworkConnection conn)
    {
        if (conn == null)
            return;

        if (players.TryGetValue(conn.ClientId, out var player))
        {
            if (player != null && player.IsSpawned)
                player.Despawn();

            players.Remove(conn.ClientId);
        }

        waitingForSpawn.Remove(conn.ClientId);
    }

    // ==========================================================

    private string GetOrCreateClass(NetworkConnection conn)
    {
        if (classByConn.TryGetValue(conn.ClientId, out var cls))
            return cls;

        classByConn[conn.ClientId] = defaultClassId;
        return defaultClassId;
    }
}
