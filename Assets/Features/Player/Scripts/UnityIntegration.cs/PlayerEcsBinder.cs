using FishNet.Object;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Features.Player.UnityIntegration;

public sealed class PlayerEcsBinder : NetworkBehaviour
{
    private Entity entity;
    private NetworkPlayer networkPlayer;

    public Entity Entity => entity;

    private void Awake()
    {
        networkPlayer = GetComponent<NetworkPlayer>();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        StartCoroutine(InitNextFrame());
    }

    private System.Collections.IEnumerator InitNextFrame()
    {
        yield return null;
        CreateEntity();
    }

    private EntityManager GetEM()
    {
        var world = World.DefaultGameObjectInjectionWorld;
        return world != null ? world.EntityManager : default;
    }

    private void CreateEntity()
    {
        var em = GetEM();
        if (em == default)
            return;

        if (entity != Entity.Null && em.Exists(entity))
            return;

        entity = em.CreateEntity(
            typeof(LocalTransform)
        );

        em.SetComponentData(entity, LocalTransform.FromPosition(transform.position));
    }

    private void Update()
    {
        if (!IsServer)
            return;

        var em = GetEM();
        if (em == default)
            return;

        if (entity == Entity.Null || !em.Exists(entity))
        {
            CreateEntity();
            return;
        }

        em.SetComponentData(entity, LocalTransform.FromPosition(transform.position));

        bool shouldBeTargetable = networkPlayer == null || networkPlayer.IsAiTargetable;
        bool hasPlayerTag = em.HasComponent<PlayerTag>(entity);

        if (shouldBeTargetable && !hasPlayerTag)
            em.AddComponent<PlayerTag>(entity);
        else if (!shouldBeTargetable && hasPlayerTag)
            em.RemoveComponent<PlayerTag>(entity);
    }

    private void OnDestroy()
    {
        if (!IsServer)
            return;

        var em = GetEM();
        if (em == default)
            return;

        if (em.Exists(entity))
            em.DestroyEntity(entity);
    }
}
