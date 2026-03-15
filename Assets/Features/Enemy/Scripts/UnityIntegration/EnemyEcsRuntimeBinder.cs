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

    [Header("Attack")]
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackCooldown = 1.5f;

    private EntityManager em;

    public override void OnStartServer()
    {
        base.OnStartServer();

        Debug.Log($"[ECS] CREATE entity for {name} | pos={transform.position}", this);

        em = World.DefaultGameObjectInjectionWorld.EntityManager;

        entity = em.CreateEntity(
            typeof(LocalTransform),
            typeof(EnemyTag),
            typeof(EnemyAI),
            typeof(EnemyState),
            typeof(EnemyTargetPosition),
            typeof(EnemyPatrolState),
            typeof(EnemyPatrolSettings),
            typeof(EnemyBlocked),

            // NEW
            typeof(EnemyAggroState),
            typeof(EnemyLastKnownPosition),
            typeof(EnemyAttackState)
        );

        Debug.Log($"[ECS] ENTITY CREATED: {entity.Index}", this);

        // ================= TRANSFORM =================
        em.SetComponentData(
            entity,
            LocalTransform.FromPosition(transform.position)
        );

        // ================= AI =================
        em.SetComponentData(entity, new EnemyAI
        {
            MoveSpeed = moveSpeed,
            AggroRadius = aggroRadius,
            LoseAggroRadius = loseAggroRadius,
            AttackRange = attackRange,
            AttackCooldown = attackCooldown
        });

        em.SetComponentData(entity, new EnemyState
        {
            Value = EnemyAIState.Patrol
        });

        em.SetComponentData(entity, new EnemyTargetPosition
        {
            Value = transform.position
        });

        // ================= NEW STATES =================
        em.SetComponentData(entity, new EnemyAggroState
        {
            Timer = 0f
        });

        em.SetComponentData(entity, new EnemyLastKnownPosition
        {
            Value = transform.position
        });

        em.SetComponentData(entity, new EnemyAttackState
        {
            Cooldown = 0f
        });

        // ================= PATROL =================
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

        // ================= PATROL POINTS =================
        var buffer = em.AddBuffer<EnemyPatrolPoint>(entity);

        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            foreach (var p in patrolPoints)
            {
                if (p == null) continue;

                buffer.Add(new EnemyPatrolPoint
                {
                    Position = p.position
                });
            }
        }
        else
        {
            Debug.LogWarning(
                "[EnemyEcsRuntimeBinder] PatrolPoints EMPTY",
                this
            );
        }

        Debug.Log($"[ECS] Patrol points: {buffer.Length}", this);
    }
}
