using UnityEngine;
using Unity.Entities;
using FishNet;
using FishNet.Object;

public class EnemyDespawnBridge : MonoBehaviour
{
    private EnemyEcsRuntimeBinder binder;
    private NetworkObject networkObject;
    private Entity entity;
    private EntityManager em;
    private bool despawnRequested;

    private void Awake()
    {
        binder = GetComponent<EnemyEcsRuntimeBinder>();
        networkObject = GetComponent<NetworkObject>();
    }

    private void Update()
    {
        if (!InstanceFinder.IsServer)
            return;

        if (despawnRequested)
            return;

        if (!TryResolveEntity())
            return;

        if (!em.HasComponent<EnemyMarkedForDespawn>(entity))
            return;

        despawnRequested = true;

        if (networkObject != null && networkObject.IsSpawned)
        {
            InstanceFinder.ServerManager.Despawn(networkObject);
            return;
        }

        Destroy(gameObject);
    }

    private bool TryResolveEntity()
    {
        if (em == default && Unity.Entities.World.DefaultGameObjectInjectionWorld != null)
            em = Unity.Entities.World.DefaultGameObjectInjectionWorld.EntityManager;

        if (em == default || binder == null)
            return false;

        if (entity != Entity.Null && em.Exists(entity))
            return true;

        if (binder.Entity == Entity.Null || !em.Exists(binder.Entity))
            return false;

        entity = binder.Entity;
        return true;
    }
}
