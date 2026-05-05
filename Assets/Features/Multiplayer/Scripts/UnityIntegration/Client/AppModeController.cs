using FishNet.Managing;
using UnityEngine;

public sealed class AppModeController : MonoBehaviour
{
    [SerializeField] private NetworkManager networkManager;

    public void StartServerOnly()
    {
        networkManager.ServerManager.StartConnection();
    }

    public void StartClientOnly(string ip, ushort port)
    {
        networkManager.TransportManager.Transport.SetClientAddress(ip);
        networkManager.TransportManager.Transport.SetPort(port);

        networkManager.ClientManager.StartConnection();
    }

    public void StartServerAndClient(ushort port)
    {
        ClientConnectionController.I?.GetFlow()?.StartConnect();
        ClientConnectionController.I?.SuppressNextStoppedEvent();

        networkManager.TransportManager.Transport.SetPort(port);
        networkManager.ServerManager.StartConnection();
        networkManager.ClientManager.StartConnection();
    }
}
