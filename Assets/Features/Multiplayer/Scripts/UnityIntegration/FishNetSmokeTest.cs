using UnityEngine;
using FishNet.Managing;

public class FishNetSmokeTest : MonoBehaviour
{
    [SerializeField] private NetworkManager networkManager;

    private void Start()
    {
        networkManager.ServerManager.StartConnection();
        networkManager.ClientManager.StartConnection();

        Debug.Log("[FishNet] StartConnection called");
    }
}
