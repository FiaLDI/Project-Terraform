using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public sealed class PlayerEcsBinder : MonoBehaviour
{
    private Entity entity;
    private EntityManager em;

    void Start()
    {
        em = World.DefaultGameObjectInjectionWorld.EntityManager;

        entity = em.CreateEntity(
            typeof(LocalTransform),
            typeof(PlayerTag)
        );

        em.SetComponentData(entity,
            LocalTransform.FromPosition(transform.position));
    }

    void Update()
    {
        if (!em.Exists(entity)) return;

        em.SetComponentData(entity,
            LocalTransform.FromPosition(transform.position));
    }
}
