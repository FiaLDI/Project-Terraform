using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using FishNet.Object;
using Features.Enemy.Data;

public sealed class EnemyEcsRuntimeBinder : NetworkBehaviour
{
    private Entity entity;
    public Entity Entity => entity;

    [SerializeField] private EnemyConfigSO config;
    [SerializeField] private Transform[] patrolPoints;

    private EntityManager GetEM()
    {
        var world = World.DefaultGameObjectInjectionWorld;
        return world != null ? world.EntityManager : default;
    }

    // =========================================================
    public override void OnStartServer()
    {
        base.OnStartServer();
        Init();
    }

    public void SetConfig(EnemyConfigSO cfg)
    {
        config = cfg;
    }

    public void ForceInit()
    {
        if (!IsServer) return;
        Init();
    }

    private void Init()
    {
        var em = GetEM();
        if (em == default || config == null)
            return;

        if (entity != Entity.Null && em.Exists(entity))
            return;

        entity = em.CreateEntity(
            typeof(LocalTransform),
            typeof(EnemyTag),
            typeof(EnemyAI),
            typeof(EnemyState),
            typeof(EnemyTargetPosition),
            typeof(EnemyAttackState),
            typeof(EnemyTarget),
            typeof(EnemyHasLineOfSight),
            typeof(EnemyRadius),
            typeof(EnemySteeringData),

            typeof(EnemyPatrolState),
            typeof(EnemyPatrolSettings),
            typeof(EnemyAggroState),
            typeof(EnemyLastKnownPosition)
        );

        em.SetComponentData(entity, new EnemyLastKnownPosition
        {
            Value = transform.position
        });

        em.AddComponentData(entity, new EnemySteeringData
        {
            seekWeight = config.seekWeight,
            avoidWeight = config.avoidWeight,
            separationWeight = config.separationWeight,
            orbitWeight = config.orbitWeight,

            avoidDistance = config.avoidDistance,
            sideAvoidDistance = config.sideAvoidDistance,
            separationRadius = config.separationRadius,

            rotationSpeed = config.rotationSpeed,
            orbitStrength = config.orbitStrength,

            enableSeparation = config.enableSeparation,
            enableAvoidance = config.enableAvoidance,
            enableOrbit = config.enableOrbit
        });

        em.SetComponentData(entity, new EnemyPatrolState
        {
            CurrentIndex = 0,
            IsWaiting = false,
            WaitTimer = 0f
        });

        em.SetComponentData(entity, new EnemyPatrolSettings
        {
            ReachDistance = 0.5f,
            MinWaitTime = 1f,
            MaxWaitTime = 2f,
            RandomPatrol = true
        });

        em.SetComponentData(entity, new EnemyAggroState
        {
            Timer = 0f
        });

        em.SetComponentData(entity,
            LocalTransform.FromPositionRotation(
                transform.position,
                transform.rotation
            ));

        em.SetComponentData(entity,
            LocalTransform.FromPosition(transform.position));

        em.SetComponentData(entity, new EnemyTarget
        {
            Value = Entity.Null
        });

        em.SetComponentData(entity, new EnemyHasLineOfSight
        {
            Value = true
        });

        
        em.SetComponentData(entity, new EnemyRadius
        {
            Value = 0.6f
        });

        em.AddBuffer<EnemyAggroElement>(entity);

        var buffer = em.AddBuffer<EnemyPatrolPoint>(entity);

        foreach (var p in patrolPoints)
        {
            if (p == null) continue;

            buffer.Add(new EnemyPatrolPoint
            {
                Position = p.position
            });
        }

        ApplyConfig(em);
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
            StopDistanceMultiplier = config.stopDistanceMultiplier,
            VisionRange = config.visionRange
        });

        em.SetComponentData(entity, new EnemyState
        {
            Value = EnemyAIState.Patrol
        });

        em.SetComponentData(entity, new EnemyTargetPosition
        {
            Value = transform.position
        });


        em.SetComponentData(entity, new EnemyAttackState
        {
            Cooldown = 0f,
            DoAttack = false
        });
    }

    // =========================================================
    private void Update()
    {
        if (!IsServer) return;

        var em = GetEM();
        if (em == default) return;

        if (entity == Entity.Null || !em.Exists(entity))
        {
            Init();
            return;
        }

        var t = em.GetComponentData<LocalTransform>(entity);

        var pos = transform.position;

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
