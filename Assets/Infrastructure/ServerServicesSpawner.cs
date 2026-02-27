using FishNet.Object;
using UnityEngine;

public class ServerServicesSpawner : NetworkBehaviour
{
    [SerializeField] private NetworkObject buffTickPrefab;
    [SerializeField] private NetworkObject dropServicePrefab;

    public override void OnStartServer()
    {
        SpawnService(buffTickPrefab);
        SpawnService(dropServicePrefab);
    }

    private void SpawnService(NetworkObject prefab)
    {
        var instance = Instantiate(prefab);
        Spawn(instance);
    }
}