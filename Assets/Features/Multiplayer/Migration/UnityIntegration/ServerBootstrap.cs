using UnityEngine;
using FishNet;
using FishNet.Managing;
using FishNet.Transporting;
using FishNet.Object;
using FishNet.Connection;
using Multiplayer.Application;
using Multiplayer.Domain;

public sealed class ServerBootstrap : MonoBehaviour
{
    [SerializeField] private NetworkObject playerPrefab;

    private IServerGameFlow flow;
    private NetworkManager net;

    private void Awake()
    {
        net = InstanceFinder.NetworkManager;
        flow = new ServerGameFlow();

        net.ServerManager.OnServerConnectionState += OnServerConnectionState;

        // 👇 ВАЖНО — правильная сигнатура
        net.ServerManager.OnRemoteConnectionState += OnRemoteConnectionState;
    }

    public void StartDedicatedServer()
    {
        flow.StartServer();
        net.ServerManager.StartConnection();
    }

    private void OnServerConnectionState(ServerConnectionStateArgs args)
    {
        if (args.ConnectionState == LocalConnectionState.Started)
        {
            flow.NotifyServerStarted();
            flow.NotifySceneLoaded();
            flow.NotifyWorldPrepared();
        }
    }

    // 👇 ПРАВИЛЬНАЯ сигнатура для твоей версии
    private void OnRemoteConnectionState(
        NetworkConnection conn,
        RemoteConnectionStateArgs args)
    {
        if (args.ConnectionState == RemoteConnectionState.Started)
        {
            SpawnPlayer(conn);
        }
        else if (args.ConnectionState == RemoteConnectionState.Stopped)
        {
            DespawnPlayer(conn);
        }
    }

    // =========================
    // SPAWN
    // =========================

    private void SpawnPlayer(NetworkConnection conn)
    {
        if (!PlayerSpawnRegistry.I.HasProvider)
        {
            Debug.LogError("No spawn providers registered!");
            return;
        }

        if (!PlayerSpawnRegistry.I.TryGetRandom(out var provider))
        {
            Debug.LogError("Failed to get spawn provider!");
            return;
        }

        provider.TryGetSpawnPoint(out var pos, out var rot);

        NetworkObject playerInstance =
            Instantiate(playerPrefab, pos, rot);

        net.ServerManager.Spawn(playerInstance, conn);

        Debug.Log($"Spawned player for {conn.ClientId}");
    }

    // =========================
    // DESPAWN
    // =========================

    private void DespawnPlayer(NetworkConnection conn)
    {
        foreach (var obj in conn.Objects)
        {
            net.ServerManager.Despawn(obj);
        }

        Debug.Log($"Despawned player for {conn.ClientId}");
    }
}
