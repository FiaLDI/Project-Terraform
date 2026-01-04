using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Features.Player.UnityIntegration;
using FishNet.Component.Transforming;

public sealed class NetworkPlayerService : MonoBehaviour
{
    public static NetworkPlayerService I { get; private set; }

    [SerializeField] private NetworkObject playerPrefab;
    [SerializeField] private string defaultClassId = "0";

    private readonly Dictionary<int, NetworkObject> _playersByConn = new();
    private readonly Dictionary<int, string> _classByConn = new();
    private readonly HashSet<int> _waitingForSpawn = new();

    private NetworkManager _nm;

    // ==========================================================
    // LIFECYCLE
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

        _nm = InstanceFinder.NetworkManager;
    }

    private void OnEnable()
    {
        if (_nm == null)
            return;

        _nm.ServerManager.OnRemoteConnectionState += OnRemoteConnectionState;
        _nm.SceneManager.OnClientLoadedStartScenes += OnClientLoadedStartScenes;
    }

    private void OnDisable()
    {
        if (_nm == null)
            return;

        _nm.ServerManager.OnRemoteConnectionState -= OnRemoteConnectionState;
        _nm.SceneManager.OnClientLoadedStartScenes -= OnClientLoadedStartScenes;
    }

    // ==========================================================
    // SPAWN ENTRY
    // ==========================================================

    private void OnClientLoadedStartScenes(NetworkConnection conn, bool asServer)
    {
        if (!asServer)
            return;

        TrySpawn(conn);
    }

    public void TrySpawn(NetworkConnection conn)
    {
        if (conn == null)
            return;

        if (_playersByConn.ContainsKey(conn.ClientId))
            return;

        if (!TryGetSpawnProvider(out var spawnProvider))
        {
            StartCoroutine(WaitForSpawnPointAndSpawn(conn));
            return;
        }

        string classId = GetOrCreateClass(conn);
        SpawnPlayer(conn, classId, spawnProvider);
    }

    // ==========================================================
    // WAIT FOR SPAWN POINT
    // ==========================================================

    private IEnumerator WaitForSpawnPointAndSpawn(NetworkConnection conn)
    {
        while (true)
        {
            // клиент мог уйти
            if (!_nm.ServerManager.Clients.ContainsKey(conn.ClientId))
                yield break;

            if (TryGetSpawnProvider(out var provider))
            {
                string classId = GetOrCreateClass(conn);
                SpawnPlayer(conn, classId, provider);
                yield break;
            }

            yield return null;
        }
    }


    // ==========================================================
    // ACTUAL SPAWN
    // ==========================================================

    private void SpawnPlayer(
        NetworkConnection conn,
        string classId,
        IPlayerSpawnProvider spawnProvider)
    {
        spawnProvider.TryGetSpawnPoint(out var pos, out var rot);

        var player = Instantiate(playerPrefab, pos, rot);

        var psn = player.GetComponent<PlayerStateNetwork>();
        psn.PreInitClass(classId);

        _nm.ServerManager.Spawn(player, conn);

        _playersByConn[conn.ClientId] = player;
    }


    // ==========================================================
    // DESPAWN
    // ==========================================================

    private void OnRemoteConnectionState(
        NetworkConnection conn,
        FishNet.Transporting.RemoteConnectionStateArgs args)
    {
        if (args.ConnectionState ==
            FishNet.Transporting.RemoteConnectionState.Stopped)
        {
            TryDespawn(conn);
        }
    }

    public void TryDespawn(NetworkConnection conn)
    {
        if (conn == null)
            return;

        if (_playersByConn.TryGetValue(conn.ClientId, out var player))
        {
            if (player != null && player.IsSpawned)
                player.Despawn();

            _playersByConn.Remove(conn.ClientId);
        }

        _waitingForSpawn.Remove(conn.ClientId);

        Debug.Log($"[PlayerService] Despawned player for conn {conn.ClientId}");
    }

    // ==========================================================
    // RESPAWN
    // ==========================================================

    public void RequestRespawn(NetworkConnection conn, float delay = 2f)
    {
        if (conn == null)
            return;

        StartCoroutine(RespawnRoutine(conn, delay));
    }

    private IEnumerator RespawnRoutine(NetworkConnection conn, float delay)
    {
        TryDespawn(conn);
        yield return new WaitForSeconds(delay);

        if (_nm.ServerManager.Clients.ContainsKey(conn.ClientId))
            TrySpawn(conn);
    }

    // ==========================================================
    // SPAWN PROVIDERS
    // ==========================================================

    private bool TryGetSpawnProvider(out IPlayerSpawnProvider provider)
    {
        var behaviours = FindObjectsByType<MonoBehaviour>(
            FindObjectsSortMode.None);

        foreach (var mb in behaviours)
        {
            if (mb is IPlayerSpawnProvider p)
            {
                provider = p;
                return true;
            }
        }

        provider = null;
        return false;
    }

    // ==========================================================
    // CLASS
    // ==========================================================

    private string GetOrCreateClass(NetworkConnection conn)
    {
        if (_classByConn.TryGetValue(conn.ClientId, out var classId))
            return classId;

        classId = defaultClassId;
        _classByConn[conn.ClientId] = classId;

        Debug.Log($"[PlayerService] Assign default class '{classId}' to {conn.ClientId}");
        return classId;
    }
}
