using FishNet;
using FishNet.Managing;
using FishNet.Transporting;
using FishNet.Managing.Scened;
using UnityEngine;

public sealed class BootstrapUI : MonoBehaviour
{
    [SerializeField] private string hubSceneName = "NetHubScene";

    private NetworkManager NM => InstanceFinder.NetworkManager;

    private void OnEnable()
    {
        NM.ServerManager.OnServerConnectionState += OnServerState;
    }

    private void OnDisable()
    {
        if (NM != null)
            NM.ServerManager.OnServerConnectionState -= OnServerState;
    }

    // =========================
    // HOST
    // =========================
    public void Host()
    {
        Debug.Log("[BootstrapUI] HOST pressed");

        NM.ServerManager.StartConnection();
        NM.ClientManager.StartConnection();
    }

    // =========================
    // CONNECT
    // =========================
    public void Connect()
    {
        Debug.Log("[BootstrapUI] CONNECT pressed");

        NM.ClientManager.StartConnection();
    }

    // =========================
    // SERVER READY CALLBACK
    // =========================
    private void OnServerState(ServerConnectionStateArgs args)
    {
        if (args.ConnectionState != LocalConnectionState.Started)
            return;

        Debug.Log("[BootstrapUI] Server started → loading Hub scene");

        LoadHubScene();
    }

    // =========================
    // SCENE LOAD
    // =========================
    private void LoadHubScene()
    {
        if (!NM.IsServer)
        {
            Debug.LogWarning("[BootstrapUI] LoadHubScene called but not server");
            return;
        }

        var data = new SceneLoadData(hubSceneName)
        {
            ReplaceScenes = ReplaceOption.All
        };

        NM.SceneManager.LoadGlobalScenes(data);
    }
}
