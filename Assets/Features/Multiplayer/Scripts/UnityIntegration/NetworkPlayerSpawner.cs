using FishNet.Connection;
using FishNet.Managing;
using UnityEngine;

public sealed class NetworkPlayerSpawner : MonoBehaviour
{
    private NetworkManager _nm;

    private void Awake()
    {
        _nm = FishNet.InstanceFinder.NetworkManager;
    }

    private void OnEnable()
    {
        if (_nm == null)
            return;

        _nm.SceneManager.OnClientLoadedStartScenes += OnClientLoadedStartScenes;
        _nm.ServerManager.OnRemoteConnectionState += OnRemoteConnectionState;
    }

    private void OnDisable()
    {
        if (_nm == null)
            return;

        _nm.SceneManager.OnClientLoadedStartScenes -= OnClientLoadedStartScenes;
        _nm.ServerManager.OnRemoteConnectionState -= OnRemoteConnectionState;
    }

    private void OnClientLoadedStartScenes(NetworkConnection conn, bool asServer)
    {
        if (!asServer)
            return;

        NetworkPlayerService.I?.TrySpawn(conn);
    }

    private void OnRemoteConnectionState(
        NetworkConnection conn,
        FishNet.Transporting.RemoteConnectionStateArgs args)
    {
        if (args.ConnectionState != FishNet.Transporting.RemoteConnectionState.Stopped)
            return;

        NetworkPlayerService.I?.TryDespawn(conn);
    }
}
