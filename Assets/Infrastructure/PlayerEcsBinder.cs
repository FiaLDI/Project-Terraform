using FishNet.Object;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public sealed class PlayerEcsBinder : NetworkBehaviour
{
    private Entity entity;
    private EntityManager em;

    public Entity Entity => entity;

    private int netId;

    public override void OnStartServer()
    {
        base.OnStartServer();

        netId = Owner.ClientId;
        Init();
    }

    private void Init()
    {
        var world = World.DefaultGameObjectInjectionWorld;
        if (world == null) return;

        em = world.EntityManager;

        if (entity != Entity.Null && em.Exists(entity))
            return;

        entity = em.CreateEntity(
            typeof(LocalTransform),
            typeof(PlayerTag)
        );

        em.SetComponentData(entity,
            LocalTransform.FromPositionRotation(
                transform.position,
                transform.rotation
            ));

        PlayerRegistryECS.Register(netId, entity);
    }

    private void Update()
    {
        if (!IsServer) return;
        if (em == default) return;

        if (entity == Entity.Null || !em.Exists(entity))
        {
            Init();
            return;
        }

        em.SetComponentData(entity,
            LocalTransform.FromPositionRotation(
                transform.position,
                transform.rotation
            ));
    }

    private void OnDestroy()
    {
        if (!IsServer) return;

        if (em != default && em.Exists(entity))
            em.DestroyEntity(entity);

        PlayerRegistryECS.Unregister(netId);
    }
}
