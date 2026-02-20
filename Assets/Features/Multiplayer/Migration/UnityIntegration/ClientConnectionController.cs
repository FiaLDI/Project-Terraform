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

    private bool hasLoggedIn;

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

    // 🔹 Для UI
    public ClientGameFlow GetFlow() => flow;

    public void Connect(string ip, ushort port)
    {
        hasLoggedIn = false;
        loginBridge = null;

        networkManager.TransportManager.Transport.SetClientAddress(ip);
        networkManager.TransportManager.Transport.SetPort(port);

        flow.StartConnect();
        networkManager.ClientManager.StartConnection();
    }

    // 🔹 Для UI
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

    // 🔥 Вызывается из ClientLoginBridge.OnStartClient()
    public void OnLoginBridgeReady(ClientLoginBridge bridge)
    {
        loginBridge = bridge;
        TrySendLogin();
    }

    private void TrySendLogin()
    {
        if (hasLoggedIn)
            return;

        if (loginBridge == null)
            return;

        hasLoggedIn = true;

        string pid = PersistentIdProvider.GetOrCreate();

        Debug.Log("[fix-net] Sending Login RPC");

        flow.NotifyLoginSent();
        loginBridge.SendLogin(pid);
    }

    public void NotifySpawned()
    {
        flow.NotifyPlayerSpawned();
    }
}
