using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using FishNet.Object;

public sealed class PlayerEcsBinder : NetworkBehaviour
{
    private Entity entity;
    public Entity Entity => entity;

    // =========================================================
    public override void OnStartServer()
    {
        base.OnStartServer();
        StartCoroutine(InitNextFrame());
    }

    private System.Collections.IEnumerator InitNextFrame()
    {
        yield return null; // ждём ECS

        CreateEntity();
    }

    // =========================================================
    private EntityManager GetEM()
    {
        var world = World.DefaultGameObjectInjectionWorld;
        return world != null ? world.EntityManager : default;
    }

    private void CreateEntity()
    {
        var em = GetEM();
        if (em == default) return;

        entity = em.CreateEntity(
            typeof(LocalTransform),
            typeof(PlayerTag)
        );

        em.SetComponentData(entity,
            LocalTransform.FromPosition(transform.position));
    }

    private void Update()
    {
        if (!IsServer) return;

        var em = GetEM();
        if (em == default) return;

        // 🔥 пересоздание после смены сцены
        if (entity == Entity.Null || !em.Exists(entity))
        {
            CreateEntity();
            return;
        }

        // 🔥 sync GameObject → ECS
        em.SetComponentData(entity,
            LocalTransform.FromPosition(transform.position));
    }

    private void OnDestroy()
    {
        if (!IsServer) return;

        var em = GetEM();
        if (em == default) return;

        if (em.Exists(entity))
            em.DestroyEntity(entity);
    }
}
