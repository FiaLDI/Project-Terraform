using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using FishNet.Object;
using Features.Enemy.Data;
using System.Collections;

public sealed class EnemyEcsRuntimeBinder : NetworkBehaviour
{
    private Entity entity;
    public Entity Entity => entity;

    [Header("Config")]
    [SerializeField] private EnemyConfigSO config;

    [Header("Patrol Points")]
    [SerializeField] private Transform[] patrolPoints;

    [Header("Patrol Settings")]
    [SerializeField] private float reachDistance = 0.6f;
    [SerializeField] private float minWait = 1f;
    [SerializeField] private float maxWait = 3f;
    [SerializeField] private bool randomPatrol = true;

    // =========================================================
    public override void OnStartServer()
    {
        base.OnStartServer();
        StartCoroutine(InitNextFrame());
    }

    private IEnumerator InitNextFrame()
    {
        yield return null; // 👈 ждём ECS world

        CreateEntity();
        FillPatrolBuffer();
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
            typeof(EnemyTag),
            typeof(EnemyAI),
            typeof(EnemyState),
            typeof(EnemyTargetPosition),
            typeof(EnemyPatrolState),
            typeof(EnemyPatrolSettings),
            typeof(EnemyBlocked),
            typeof(EnemyTarget),
            typeof(EnemyAggroState),
            typeof(EnemyLastKnownPosition),
            typeof(EnemyAttackState),
            typeof(EnemyHasLineOfSight)
        );

        em.SetComponentData(entity, LocalTransform.FromPosition(transform.position));

        em.SetComponentData(entity, new EnemyTarget { Value = Entity.Null });

        em.SetComponentData(entity, new EnemyHasLineOfSight { Value = true });

        ApplyConfig(em);

        em.SetComponentData(entity, new EnemyState { Value = EnemyAIState.Patrol });

        em.SetComponentData(entity, new EnemyTargetPosition
        {
            Value = transform.position
        });

        em.SetComponentData(entity, new EnemyAggroState { Timer = 0f });

        em.SetComponentData(entity, new EnemyLastKnownPosition
        {
            Value = transform.position
        });

        em.SetComponentData(entity, new EnemyAttackState
        {
            Cooldown = 0f,
            DoAttack = false
        });

        em.SetComponentData(entity, new EnemyBlocked { Value = false });

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

        em.AddBuffer<EnemyPatrolPoint>(entity);
        em.AddBuffer<EnemyAggroElement>(entity);
        em.AddBuffer<DamageEvent>(entity);
    }

    private void FillPatrolBuffer()
    {
        var em = GetEM();
        if (em == default || !em.Exists(entity)) return;

        var buffer = em.GetBuffer<EnemyPatrolPoint>(entity);

        if (patrolPoints == null) return;

        foreach (var p in patrolPoints)
        {
            if (p == null) continue;

            buffer.Add(new EnemyPatrolPoint
            {
                Position = p.position
            });
        }
    }

    // =========================================================
    private void Update()
    {
        if (!IsServer) return;

        var em = GetEM();
        if (em == default) return;

        // 🔥 если entity умерла (смена сцены)
        if (entity == Entity.Null || !em.Exists(entity))
        {
            CreateEntity();
            FillPatrolBuffer();
            return;
        }

        // 🔥 sync ECS → GameObject
        var transformData = em.GetComponentData<LocalTransform>(entity);
        transform.position = transformData.Position;
    }
    public void SetConfig(EnemyConfigSO cfg)
    {
        config = cfg;

        if (!IsServer)
            return;

        var em = GetEM();
        if (em == default)
            return;

        if (entity != Entity.Null && em.Exists(entity))
        {
            ApplyConfig(em);
        }
    }

    private void ApplyConfig(EntityManager em)
    {
        em.SetComponentData(entity, new EnemyAI
        {
            MoveSpeed = 3f,
            AggroRadius = config.aggroRadius,
            LoseAggroRadius = config.aggroRadius * 1.5f,
            AttackRange = config.attackRange,
            AttackCooldown = config.attackCooldown,
            AttackEnterOffset = config.attackEnterOffset,
            AttackExitOffset = config.attackExitOffset,
            StopDistanceMultiplier = config.stopDistanceMultiplier,
            VisionAngle = config.visionAngle,
            VisionRange = config.visionRange,
            RequireLOS = config.requireLineOfSight,
            ObstacleMask = config.obstacleMask.value
        });
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
