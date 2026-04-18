using UnityEngine;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Transporting;
using FishNet.Managing.Scened;
using FishNet.Object;

public sealed class ServerBootstrap : MonoBehaviour
{
    [SerializeField] private string hubSceneName = "NetHubScene";
    [SerializeField] private NetworkObject connectionObjectPrefab;

    private NetworkManager net;

    private void Awake()
    {
        net = InstanceFinder.NetworkManager;

        net.ServerManager.OnServerConnectionState += OnServerState;
        net.ServerManager.OnRemoteConnectionState += OnRemoteConnectionState;
    }

    private void OnDestroy()
    {
        if (net == null)
            return;

        net.ServerManager.OnServerConnectionState -= OnServerState;
        net.ServerManager.OnRemoteConnectionState -= OnRemoteConnectionState;
    }

    public void StartDedicatedServer()
    {
        ServerCompositionRoot.I.Flow.StartServer();
        net.ServerManager.StartConnection();
    }

    private void OnServerState(ServerConnectionStateArgs args)
    {
        if (args.ConnectionState != LocalConnectionState.Started)
            return;

        Debug.Log("[fix-net] Server started");

        ServerCompositionRoot.I.Flow.NotifyServerStarted();

        // Загружаем хаб-сцену
        var data = new SceneLoadData(hubSceneName)
        {
            ReplaceScenes = ReplaceOption.All
        };

        net.SceneManager.LoadGlobalScenes(data);
    }

    private void OnRemoteConnectionState(NetworkConnection conn, RemoteConnectionStateArgs args)
    {
        if (args.ConnectionState != RemoteConnectionState.Started)
            return;

        Debug.Log($"[fix-net] Client connected: {conn.ClientId}");

        StartCoroutine(SpawnConnectionObject(conn));
    }

    private System.Collections.IEnumerator SpawnConnectionObject(NetworkConnection conn)
    {
        while (!conn.LoadedStartScenes())
            yield return null;

        if (!conn.IsActive)
            yield break;

        var obj = Instantiate(connectionObjectPrefab);
        net.ServerManager.Spawn(obj, conn);

        Debug.Log($"[fix-net] ConnectionObject spawned for {conn.ClientId}");
    }
}
