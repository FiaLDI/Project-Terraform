using UnityEngine;
using FishNet.Managing;
using FishNet.Transporting;
using Multiplayer.Application;

public sealed class ClientConnectionController : MonoBehaviour
{
    public static ClientConnectionController I;

    [SerializeField] private NetworkManager networkManager;

    private ClientGameFlow flow;
    private ClientLoginBridge loginBridge;

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

    public ClientGameFlow GetFlow() => flow;

    public void Connect(string ip, ushort port)
    {
        networkManager.TransportManager.Transport.SetClientAddress(ip);
        networkManager.TransportManager.Transport.SetPort(port);

        flow.StartConnect();
        networkManager.ClientManager.StartConnection();
    }

    public void Disconnect()
    {
        flow.NotifyDisconnected();
        networkManager.ClientManager.StopConnection();
    }

    private void OnClientState(ClientConnectionStateArgs args)
    {
        if (args.ConnectionState == LocalConnectionState.Started)
        {
            flow.NotifyConnected();
        }
        else if (args.ConnectionState == LocalConnectionState.Stopped)
        {
            flow.NotifyDisconnected();
        }
    }

    // 🔥 вызывается bridge когда он появился
    public void OnLoginBridgeReady(ClientLoginBridge bridge)
    {
        loginBridge = bridge;
        SendLogin();
    }

    private void SendLogin()
    {
        if (loginBridge == null)
        {
            Debug.LogWarning("LoginBridge not ready yet");
            return;
        }

        string pid = PersistentIdProvider.GetOrCreate();

        flow.NotifyLoginSent();
        loginBridge.SendLogin(pid);
    }

    public void NotifySpawned()
    {
        flow.NotifyPlayerSpawned();
    }
}
