
using FishNet.Object;
using UnityEngine;

public sealed class WorldServicesSpawner : NetworkBehaviour
{
    [SerializeField] private NetworkObject worldProviderPrefab;
    [SerializeField] private NetworkObject generatorPrefab;

    private NetworkObject worldProviderInstance;
    private NetworkObject generatorInstance;

    public override void OnStartServer()
    {
        worldProviderInstance = SpawnService(worldProviderPrefab);
        generatorInstance = SpawnService(generatorPrefab);
    }

    private NetworkObject SpawnService(NetworkObject prefab)
    {
        var instance = Instantiate(prefab);
        Spawn(instance);
        return instance;
    }
}