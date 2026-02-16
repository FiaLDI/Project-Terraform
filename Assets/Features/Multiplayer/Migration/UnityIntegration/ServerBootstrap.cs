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
    [SerializeField] private NetworkObject loginBridgePrefab;

    private NetworkManager net;

    private void Awake()
    {
        net = InstanceFinder.NetworkManager;

        net.ServerManager.OnServerConnectionState += OnServerState;
        net.ServerManager.OnRemoteConnectionState += OnRemoteConnectionState;
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

        Debug.Log("[ServerBootstrap] Server started");

        ServerCompositionRoot.I.Flow.NotifyServerStarted();

        // 🔥 ГРУЗИМ ХАБ
        var data = new SceneLoadData(hubSceneName)
        {
            ReplaceScenes = ReplaceOption.All
        };

        net.SceneManager.LoadGlobalScenes(data);
    }

    private void OnRemoteConnectionState(NetworkConnection conn, RemoteConnectionStateArgs args)
    {
        if (args.ConnectionState == RemoteConnectionState.Started)
            StartCoroutine(SpawnBridgeWhenReady(conn));
    }

    private System.Collections.IEnumerator SpawnBridgeWhenReady(NetworkConnection conn)
    {
        while (!conn.LoadedStartScenes())
            yield return null;

        var bridge = Instantiate(loginBridgePrefab);
        net.ServerManager.Spawn(bridge, conn);

        Debug.Log($"[Server] LoginBridge spawned for {conn.ClientId}");
    }
}
