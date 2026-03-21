using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using FishNet.Object;
using Features.Enemy.Data;

public sealed class EnemyEcsRuntimeBinder : NetworkBehaviour
{
    private Entity entity;
    public Entity Entity => entity;

    [Header("Config")]
    [SerializeField] private EnemyConfigSO config;

    [Header("Patrol Points (scene transforms)")]
    [SerializeField] private Transform[] patrolPoints;

    [Header("Patrol Settings")]
    [SerializeField] private float reachDistance = 0.6f;
    [SerializeField] private float minWait = 1f;
    [SerializeField] private float maxWait = 3f;
    [SerializeField] private bool randomPatrol = true;

    private EntityManager em;

    public override void OnStartServer()
    {
        base.OnStartServer();

        if (config == null)
        {
            Debug.LogError("EnemyConfigSO is missing!", this);
            return;
        }

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

            typeof(EnemyAggroState),
            typeof(EnemyLastKnownPosition),
            typeof(EnemyAttackState),
            typeof(EnemyHasLineOfSight)
        );

        em.SetComponentData(entity, new EnemyHasLineOfSight
        {
            Value = true
        });

        // ================= TRANSFORM =================
        em.SetComponentData(entity,
            LocalTransform.FromPosition(transform.position));

        // ================= AI =================
        em.SetComponentData(entity, new EnemyAI
        {
            // todo: stats
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

        // ================= STATE =================
        em.SetComponentData(entity, new EnemyState
        {
            Value = EnemyAIState.Patrol
        });

        em.SetComponentData(entity, new EnemyTargetPosition
        {
            Value = transform.position
        });

        // ================= STATES =================
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
            Cooldown = 0f,
            DoAttack = false
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

        Debug.Log($"[ECS] Enemy initialized: {entity.Index}", this);
    }

    public void SetConfig(EnemyConfigSO cfg)
    {
        config = cfg;
    }
}