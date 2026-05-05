using UnityEngine;
using FishNet.Managing;
using FishNet.Transporting;
using Multiplayer.Application;

public sealed class ClientConnectionController : MonoBehaviour
{
    public static ClientConnectionController I;

    [SerializeField] private NetworkManager networkManager;

    private ClientGameFlow flow;
    private bool suppressNextStoppedEvent;

    private void Awake()
    {
        if (I != null)
        {
            Destroy(gameObject);
            return;
        }

        I = this;
        DontDestroyOnLoad(gameObject);

        flow = new ClientGameFlow();
        flow.OnStateChanged += s => Debug.Log($"[ClientFlow] → {s}");

        networkManager.ClientManager.OnClientConnectionState += OnClientState;
    }

    private void OnDestroy()
    {
        if (networkManager != null && networkManager.ClientManager != null)
            networkManager.ClientManager.OnClientConnectionState -= OnClientState;

        if (I == this)
            I = null;
    }

    public ClientGameFlow GetFlow() => flow;

    public void SuppressNextStoppedEvent()
    {
        suppressNextStoppedEvent = true;
    }

    public void Connect(string ip, ushort port)
    {
        networkManager.TransportManager.Transport.SetClientAddress(ip);
        networkManager.TransportManager.Transport.SetPort(port);

        flow.StartConnect();
        networkManager.ClientManager.StartConnection();
    }

    public void Disconnect()
    {
        networkManager.ClientManager.StopConnection();
    }

    private void OnClientState(ClientConnectionStateArgs args)
    {
        if (args.ConnectionState == LocalConnectionState.Started)
        {
            suppressNextStoppedEvent = false;
            flow.NotifyConnected();
        }
        else if (args.ConnectionState == LocalConnectionState.Stopped)
        {
            if (suppressNextStoppedEvent)
            {
                suppressNextStoppedEvent = false;
                Debug.Log("[ClientConnectionController] Suppressed stale stopped event.");
                return;
            }

            flow.NotifyDisconnected();

            if (!SceneTransitionService.IsReturnToMainMenuInProgress)
                SceneTransitionService.LoadMainMenuSceneLocal();
        }
    }
}
