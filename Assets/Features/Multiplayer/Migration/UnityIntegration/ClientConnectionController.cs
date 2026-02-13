using UnityEngine;
using FishNet.Managing;
using FishNet.Transporting;
using Multiplayer.Application;
using FishNet.Object;

public sealed class ClientConnectionController : NetworkBehaviour
{
    public static ClientConnectionController I;

    [SerializeField] private NetworkManager networkManager;

    private ClientGameFlow flow;

    private void Awake()
    {
        I = this;

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
            SendLogin();
        }
    }

    private void SendLogin()
    {
        string pid = PersistentIdProvider.GetOrCreate();
        LoginRpc(pid);
    }

    [ServerRpc]
    private void LoginRpc(string persistentId)
    {
        ServerLoginHandler.I.HandleLogin(Owner, persistentId);
    }
}
