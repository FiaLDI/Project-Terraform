using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public sealed class EnemyEcsRuntimeBinder : MonoBehaviour
{
    private Entity entity;
    private EntityManager em;

    [Header("AI")]
    public float moveSpeed = 3f;
    public float aggroRadius = 8f;

    private void Start()
    {
        em = World.DefaultGameObjectInjectionWorld.EntityManager;

        entity = em.CreateEntity(
            typeof(LocalTransform),
            typeof(EnemyTag),
            typeof(EnemyAI),
            typeof(EnemyState),
            typeof(EnemyTargetPosition)
        );

        em.SetComponentData(entity, LocalTransform.FromPosition(transform.position));

        em.SetComponentData(entity, new EnemyAI
        {
            MoveSpeed = moveSpeed,
            AggroRadius = aggroRadius
        });

        em.SetComponentData(entity, new EnemyState
        {
            Value = EnemyAIState.Patrol
        });

        em.SetComponentData(entity, new EnemyTargetPosition
        {
            Value = transform.position
        });
    }

    private void Update()
    {
        if (!em.Exists(entity))
            return;

        // синхронизация GO ← ECS (позиция)
        var lt = em.GetComponentData<LocalTransform>(entity);
        lt.Position = transform.position;
        em.SetComponentData(entity, lt);
    }

    private void OnDestroy()
    {
        if (em.Exists(entity))
            em.DestroyEntity(entity);
    }
}
