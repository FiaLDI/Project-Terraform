using FishNet.Managing;
using FishNet.Object;
using FishNet.Transporting;
using UnityEngine;

public class NetworkSpawnTest : MonoBehaviour
{
    [SerializeField] private NetworkObject networkPlayerPrefab;

    private void Start()
    {
        var nm = FindObjectOfType<NetworkManager>();

        nm.ServerManager.StartConnection();
        nm.ClientManager.StartConnection();

        nm.ServerManager.OnServerConnectionState += args =>
        {
            if (args.ConnectionState == LocalConnectionState.Started)
            {
                Spawn();
            }
        };
    }

    private void Spawn()
    {
        var nm = FindObjectOfType<NetworkManager>();
        var player = Instantiate(networkPlayerPrefab);
        nm.ServerManager.Spawn(player);
    }
}
