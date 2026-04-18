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
    [SerializeField] private LayerMask obstacleMask;

    private EntityManager GetEM()
    {
        var world = World.DefaultGameObjectInjectionWorld;
        return world != null ? world.EntityManager : default;
    }

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
            seekWeight = config.ai.seekWeight,
            avoidWeight = config.ai.avoidWeight,
            separationWeight = config.ai.separationWeight,
            orbitWeight = config.ai.orbitWeight,

            avoidDistance = config.ai.avoidDistance,
            sideAvoidDistance = config.ai.sideAvoidDistance,
            separationRadius = config.ai.separationRadius,

            rotationSpeed = config.ai.rotationSpeed,
            orbitStrength = config.ai.orbitStrength,

            enableSeparation = config.ai.enableSeparation,
            enableAvoidance = config.ai.enableAvoidance,
            enableOrbit = config.ai.enableOrbit
        });

        em.AddComponentData(entity, new EnemyDespawnDistance
        {
            Value = 80f
        });

        em.SetComponentData(entity, new EnemyPatrolState
        {
            CurrentIndex = 0,
            IsWaiting = false,
            WaitTimer = 0f,
            CurrentWaitDuration = 0f
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
        if (obstacleMask.value == 0)
            obstacleMask = ~0;

        em.SetComponentData(entity, new EnemyAI
        {
            MoveSpeed = config.ai.moveSpeed,
            AggroRadius = config.ai.aggroRadius,
            LoseAggroRadius = config.ai.loseAggroRadius,
            AttackRange = config.combat.attackRange,
            AttackCooldown = config.combat.attackCooldown,
            AttackEnterOffset = config.combat.attackEnterOffset,
            AttackExitOffset = config.combat.attackExitOffset,
            StopDistanceMultiplier = config.combat.stopDistanceMultiplier,
            VisionAngle = config.ai.visionAngle,
            VisionRange = config.ai.visionRange,
            RequireLOS = config.ai.requireLineOfSight,
            ObstacleMask = obstacleMask.value
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
            DoAttack = false,
            IsAttacking = false,
            Cooldown = 0f,
            Timer = 0f,
            Type = EnemyAttackType.None
        });
    }

    private void Update()
    {
        if (!IsServer) return;

        var em = GetEM();
        if (em == default) return;

        if (entity == Entity.Null || !em.Exists(entity))
        {
            Init();
        }
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
