using FishNet.Object;
using UnityEngine;

public class ServerServicesSpawner : NetworkBehaviour
{
    [SerializeField] private NetworkObject buffTickPrefab;
    [SerializeField] private NetworkObject dropServicePrefab;
    [SerializeField] private NetworkObject AbilityServicePrefab;

    public override void OnStartServer()
    {
        SpawnService(buffTickPrefab);
        SpawnService(dropServicePrefab);
        SpawnService(AbilityServicePrefab);
    }

    private void SpawnService(NetworkObject prefab)
    {
        var instance = Instantiate(prefab);
        Spawn(instance);
    }
}