using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using FishNet.Object;
using Features.Enemy.Data;
using Features.Effects.Domain;

public sealed class EnemyEcsRuntimeBinder : NetworkBehaviour
{
    private Entity entity;
    public Entity Entity => entity;
    public EnemyConfigSO Config => config;

    [SerializeField] private EnemyConfigSO config;
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private float despawnDistance = 80f;

    private EntityManager GetEM()
    {
        var world = Unity.Entities.World.DefaultGameObjectInjectionWorld;
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
        ApplyRuntimeConfig(resetState: false);
        ApplyCombatConfig();
        GetComponent<EnemyVisualController>()?.RefreshAnimatorBinding();
    }

    public void SetDespawnDistance(float distance)
    {
        despawnDistance = Mathf.Max(1f, distance);

        var em = GetEM();
        if (em == default)
            return;

        if (entity == Entity.Null || !em.Exists(entity))
            return;

        if (em.HasComponent<EnemyDespawnDistance>(entity))
        {
            em.SetComponentData(entity, new EnemyDespawnDistance
            {
                Value = despawnDistance
            });
        }
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
            typeof(EnemyAggroSettings),
            typeof(EnemyLastKnownPosition),
            typeof(EnemyPerceptionState),
            typeof(EnemyStateFrameLock)
        );

        em.SetComponentData(entity, new EnemyLastKnownPosition
        {
            Value = transform.position
        });

        em.AddComponentData(entity, new EnemyDespawnDistance
        {
            Value = despawnDistance
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

        em.SetComponentData(entity, new EnemyPerceptionState
        {
            HasTarget = false,
            HasValidLos = true,
            InsideVisionCone = true,
            DistToTarget = 9999f,
            TargetPosition = transform.position,
            PreferredCombatDistance = 0f,
            RetreatDistance = 0f,
            AttackEnterDistance = 0f,
            AttackExitDistance = 0f
        });
        em.SetComponentEnabled<EnemyStateFrameLock>(entity, false);

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
        em.AddBuffer<DamageEvent>(entity);

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
        ApplyRuntimeConfig(em, resetState: true);
    }

    private void ApplyRuntimeConfig(bool resetState)
    {
        var em = GetEM();
        if (em == default || entity == Entity.Null || !em.Exists(entity) || config == null)
            return;

        ApplyRuntimeConfig(em, resetState);
    }

    private void ApplyRuntimeConfig(EntityManager em, bool resetState)
    {
        ApplySteeringConfig(em);
        ApplyAggroSettings(em);
        ApplyAIConfig(em);

        if (!resetState)
            return;

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

        ApplyCombatConfig();
    }

    private void ApplySteeringConfig(EntityManager em)
    {
        em.SetComponentData(entity, new EnemySteeringData
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
            directionSmoothing = config.ai.directionSmoothing,
            enableSeparation = config.ai.enableSeparation,
            enableAvoidance = config.ai.enableAvoidance,
            enableOrbit = config.ai.enableOrbit
        });
    }

    private void ApplyAggroSettings(EntityManager em)
    {
        em.SetComponentData(entity, new EnemyAggroSettings
        {
            ThreatDecayPerSecond = config.ai.threatDecayPerSecond,
            TargetSwitchThreshold = Mathf.Max(1f, config.ai.targetSwitchThreshold),
            CurrentTargetBias = Mathf.Max(0f, config.ai.currentTargetBias),
            LoseDistance = Mathf.Max(config.ai.aggroRadius, config.ai.loseAggroRadius)
        });
    }

    private void ApplyAIConfig(EntityManager em)
    {
        int resolvedObstacleMask = obstacleMask.value != 0
            ? obstacleMask.value
            : Physics.DefaultRaycastLayers;

        em.SetComponentData(entity, new EnemyAI
        {
            MoveSpeed = config.ai.moveSpeed,
            AggroRadius = config.ai.aggroRadius,
            LoseAggroRadius = config.ai.loseAggroRadius,
            AttackRange = config.combat.attackRange,
            AttackCooldown = config.combat.attackCooldown,
            AttackType = ResolveAttackType(),
            AttackEnterOffset = config.combat.attackEnterOffset,
            AttackExitOffset = config.combat.attackExitOffset,
            StopDistanceMultiplier = config.combat.stopDistanceMultiplier,
            PreferredCombatDistance = Mathf.Max(0f, config.ai.preferredCombatDistance),
            RetreatDistance = Mathf.Max(0f, config.ai.retreatDistance),
            ReengageDistance = Mathf.Max(0f, config.ai.reengageDistance),
            AggroConfirmTime = Mathf.Max(0f, config.ai.aggroConfirmTime),
            LostSightGraceTime = Mathf.Max(0f, config.ai.lostSightGraceTime),
            AttackMoveGoalTolerance = Mathf.Max(0f, config.ai.attackMoveGoalTolerance),
            ReturnReachDistance = Mathf.Max(0.05f, config.ai.returnReachDistance),
            VisionAngle = config.ai.visionAngle,
            VisionRange = config.ai.visionRange,
            RequireLOS = config.ai.requireLineOfSight,
            ObstacleMask = resolvedObstacleMask
        });
    }

    private EnemyAttackType ResolveAttackType()
    {
        if (config == null || config.combat == null)
            return EnemyAttackType.Melee;

        if (config.combat.attackType != EnemyAttackType.None)
            return config.combat.attackType;

        switch (config.combat.attackEffect.type)
        {
            case EffectType.HitscanDamage:
            case EffectType.DealDamageHitscan:
            case EffectType.SpawnProjectile:
                return EnemyAttackType.Ranged;
        }

        return config.combat.attackRange > 3f
            ? EnemyAttackType.Ranged
            : EnemyAttackType.Melee;
    }

    private void ApplyCombatConfig()
    {
        if (config == null)
            return;

        var attackHandler = GetComponent<EnemyAttackHandler>();
        attackHandler?.ApplyCombatConfig(config.combat);
    }

    private void Update()
    {
        if (!IsServer) return;

        var em = GetEM();
        if (em == default) return;

        if (entity == Entity.Null || !em.Exists(entity))
            Init();
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
