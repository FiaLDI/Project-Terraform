using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using FishNet.Object;

public sealed class EnemyEcsRuntimeBinder : NetworkBehaviour
{
    private Entity entity;
    public Entity Entity => entity;

    [Header("Patrol Points (scene transforms)")]
    [SerializeField] private Transform[] patrolPoints;

    [Header("Patrol Settings")]
    [SerializeField] private float reachDistance = 0.6f;
    [SerializeField] private float minWait = 1f;
    [SerializeField] private float maxWait = 3f;
    [SerializeField] private bool randomPatrol = true;

    [Header("AI")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float aggroRadius = 8f;
    [SerializeField] private float loseAggroRadius = 12f;

    private EntityManager em;

    public override void OnStartServer()
    {
        base.OnStartServer();

        em = World.DefaultGameObjectInjectionWorld.EntityManager;

        // =========================
        // CREATE ECS ENTITY (SERVER ONLY)
        // =========================
        entity = em.CreateEntity(
            typeof(LocalTransform),
            typeof(EnemyTag),
            typeof(EnemyAI),
            typeof(EnemyState),
            typeof(EnemyTargetPosition),
            typeof(EnemyPatrolState),
            typeof(EnemyPatrolSettings),
            typeof(EnemyBlocked)
        );

        // =========================
        // BASE TRANSFORM
        // =========================
        em.SetComponentData(
            entity,
            LocalTransform.FromPosition(transform.position)
        );

        // =========================
        // AI CONFIG
        // =========================
        em.SetComponentData(entity, new EnemyAI
        {
            MoveSpeed = moveSpeed,
            AggroRadius = aggroRadius,
            LoseAggroRadius = loseAggroRadius
        });

        em.SetComponentData(entity, new EnemyState
        {
            Value = EnemyAIState.Patrol
        });

        // ❗❗ КРИТИЧЕСКИ ВАЖНО
        // НЕ задаём цель "вперёд"
        // Цель управляется ТОЛЬКО EnemyAISystem
        em.SetComponentData(entity, new EnemyTargetPosition
        {
            Value = transform.position
        });

        // =========================
        // PATROL STATE
        // =========================
        em.SetComponentData(entity, new EnemyPatrolState
        {
            CurrentIndex = 0,
            IsWaiting = false,
            WaitTimer = 0f,
            CurrentWaitDuration = 0f
        });

        em.SetComponentData(entity, new EnemyPatrolSettings
        {
            ReachDistance = reachDistance,
            MinWaitTime = minWait,
            MaxWaitTime = maxWait,
            RandomPatrol = randomPatrol
        });

        em.SetComponentData(entity, new EnemyBlocked
        {
            Value = false
        });

        // =========================
        // PATROL POINTS BUFFER
        // =========================
        var buffer = em.AddBuffer<EnemyPatrolPoint>(entity);

        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            foreach (var p in patrolPoints)
            {
                if (p == null)
                    continue;

                buffer.Add(new EnemyPatrolPoint
                {
                    Position = p.position
                });
            }
        }
        else
        {
            Debug.LogWarning(
                "[EnemyEcsRuntimeBinder] PatrolPoints array is EMPTY. Enemy will not patrol.",
                this
            );
        }

        Debug.Log(
            $"[EnemyEcsRuntimeBinder] ECS Entity CREATED | index={entity.Index} | patrolPoints={buffer.Length}",
            this
        );
    }
}
