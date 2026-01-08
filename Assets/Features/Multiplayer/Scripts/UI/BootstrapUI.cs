using FishNet;
using FishNet.Managing;
using FishNet.Managing.Scened;
using FishNet.Transporting;
using FishNet.Connection;
using UnityEngine;

public sealed class BootstrapUI : MonoBehaviour
{
    [SerializeField] private string hubSceneName = "NetHubScene";

    private NetworkManager _nm;

    private void Start()
    {
        _nm = InstanceFinder.NetworkManager;

        if (_nm == null)
        {
            Debug.LogError("[BootstrapUI] NetworkManager not found");
            enabled = false;
            return;
        }
    }

    public void Host()
    {
        if (_nm.ServerManager.Started)
            return;

        _nm.ServerManager.StartConnection();
        _nm.ClientManager.StartConnection();

        _nm.ServerManager.OnRemoteConnectionState += OnRemoteConnection;
    }

    public void Connect()
    {
        if (_nm.ClientManager.Started)
            return;

        _nm.ClientManager.StartConnection();
    }

    private void OnRemoteConnection(
        NetworkConnection conn,
        RemoteConnectionStateArgs args)
    {
        if (args.ConnectionState != RemoteConnectionState.Started)
            return;

        _nm.ServerManager.OnRemoteConnectionState -= OnRemoteConnection;
        LoadHubScene();
    }

    private void LoadHubScene()
    {
        if (!_nm.IsServer)
            return;

        var data = new SceneLoadData(hubSceneName)
        {
            ReplaceScenes = ReplaceOption.All
        };

        _nm.SceneManager.LoadGlobalScenes(data);
    }
}
