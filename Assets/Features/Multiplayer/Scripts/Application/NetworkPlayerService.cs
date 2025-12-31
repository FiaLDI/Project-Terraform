using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Features.Player.UnityIntegration;

public sealed class NetworkPlayerService : MonoBehaviour
{
    public static NetworkPlayerService I { get; private set; }

    [SerializeField] private NetworkObject playerPrefab;

    private readonly Dictionary<int, NetworkObject> _playersByConn = new();
    private readonly Dictionary<int, string> _classByConn = new();

    [SerializeField] private string defaultClassId = "0";

    private NetworkManager _nm;

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
        if (_nm == null) return;

        _nm.ServerManager.OnRemoteConnectionState += OnRemoteConnectionState;
        _nm.SceneManager.OnClientLoadedStartScenes += OnClientLoadedStartScenes;
    }

    private void OnDisable()
    {
        if (_nm == null) return;

        _nm.ServerManager.OnRemoteConnectionState -= OnRemoteConnectionState;
        _nm.SceneManager.OnClientLoadedStartScenes -= OnClientLoadedStartScenes;
    }

    // ==========================================================
    // SPAWN
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

        string classId = GetOrCreateClass(conn);

        SpawnPlayer(conn, classId);
    }

    private void SpawnPlayer(NetworkConnection conn, string classId)
    {
        ScenePlayerSpawnPoint sp = GetSpawnPoint();

        Vector3 pos = sp ? sp.transform.position : Vector3.zero;
        Quaternion rot = sp ? sp.transform.rotation : Quaternion.identity;

        NetworkObject player = Instantiate(playerPrefab, pos, rot);

        var psn = player.GetComponent<PlayerStateNetwork>();
        psn.PreInitClass(classId); // 🔥 ключевая строка

        _nm.ServerManager.Spawn(player, conn);

        _playersByConn[conn.ClientId] = player;

        Debug.Log($"[PlayerService] Spawned player {conn.ClientId} with class '{classId}'");
    }

    // ==========================================================
    // DESPAWN
    // ==========================================================
    private void OnRemoteConnectionState(
        NetworkConnection conn,
        FishNet.Transporting.RemoteConnectionStateArgs args)
    {
        if (args.ConnectionState !=
            FishNet.Transporting.RemoteConnectionState.Stopped)
            return;

        TryDespawn(conn);
    }

    public void TryDespawn(NetworkConnection conn)
    {
        if (conn == null)
            return;

        if (!_playersByConn.TryGetValue(conn.ClientId, out var player))
            return;

        if (player != null && player.IsSpawned)
            player.Despawn();

        _playersByConn.Remove(conn.ClientId);

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

        // Клиент мог уже выйти
        if (!_nm.ServerManager.Clients.ContainsKey(conn.ClientId))
            yield break;

        TrySpawn(conn);
    }

    // ==========================================================
    // SPAWN POINTS
    // ==========================================================
    private ScenePlayerSpawnPoint GetSpawnPoint()
    {
        ScenePlayerSpawnPoint[] points =
            FindObjectsByType<ScenePlayerSpawnPoint>(
                FindObjectsSortMode.None);

        if (points.Length == 0)
            return null;

        // пока простой рандом
        return points[Random.Range(0, points.Length)];
    }

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
