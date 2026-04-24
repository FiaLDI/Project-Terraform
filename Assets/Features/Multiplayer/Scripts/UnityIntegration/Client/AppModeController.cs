using UnityEngine;
using FishNet.Managing;

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

        // Устанавливаем порт
        networkManager.TransportManager.Transport.SetPort(port);

        // Стартуем сервер
        networkManager.ServerManager.StartConnection();

        // Стартуем клиент
        networkManager.ClientManager.StartConnection();
    }
}
