using UnityEngine;
using Unity.Entities;
using FishNet;

public class EnemyDespawnBridge : MonoBehaviour
{
    private Entity entity;
    private EntityManager em;

    private void Start()
    {
        em = World.DefaultGameObjectInjectionWorld.EntityManager;

        var binder = GetComponent<EnemyEcsRuntimeBinder>();
        if (binder != null)
            entity = binder.Entity;
    }

    private void Update()
    {
        if (entity == Entity.Null || !em.Exists(entity))
            return;

        if (em.HasComponent<EnemyMarkedForDespawn>(entity))
        {
            InstanceFinder.ServerManager.Despawn(gameObject); // 🔥 СНАЧАЛА GO
        }
    }
}
