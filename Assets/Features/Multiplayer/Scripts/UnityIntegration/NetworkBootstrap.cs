using FishNet.Managing;
using UnityEngine;

public class NetworkBootstrap : MonoBehaviour
{
    private void Start()
    {
        var nm = FindObjectOfType<NetworkManager>();

        Debug.Log("NetworkBootstrap start");

        nm.ServerManager.StartConnection();
        nm.ClientManager.StartConnection();
    }
}
