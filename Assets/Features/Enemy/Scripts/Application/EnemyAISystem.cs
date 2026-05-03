using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(EnemyLOSSystemECS))]
public partial struct EnemyAISystem : ISystem
{
    private ComponentLookup<LocalTransform> transformLookup;
    private ComponentLookup<PlayerDead> deadLookup;

    public void OnCreate(ref SystemState state)
    {
        transformLookup = state.GetComponentLookup<LocalTransform>(true);
        deadLookup = state.GetComponentLookup<PlayerDead>(true);
    }

    public void OnUpdate(ref SystemState state)
    {
        var em = state.EntityManager;
        transformLookup.Update(ref state);
        deadLookup.Update(ref state);

        float deltaTime = SystemAPI.Time.DeltaTime;

        foreach (var (attackState, perception, ai, transform, enemyTarget, lineOfSight, entity) in SystemAPI.Query<
                     RefRW<EnemyAttackState>,
                     RefRW<EnemyPerceptionState>,
                     RefRO<EnemyAI>,
                     RefRO<LocalTransform>,
                     RefRO<EnemyTarget>,
                     RefRO<EnemyHasLineOfSight>>()
                     .WithEntityAccess()
                     .WithNone<EnemyInactive>())
        {
            em.SetComponentEnabled<EnemyStateFrameLock>(entity, false);

            if (attackState.ValueRW.Cooldown > 0f)
                attackState.ValueRW.Cooldown = math.max(0f, attackState.ValueRO.Cooldown - deltaTime);

            perception.ValueRW = EnemyBrainUtility.BuildPerception(
                ai.ValueRO,
                transform.ValueRO,
                enemyTarget.ValueRO,
                lineOfSight.ValueRO,
                transformLookup,
                deadLookup
            );
        }
    }
}

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(EnemyAISystem))]
public partial struct EnemyPatrolStateSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var em = state.EntityManager;
        float deltaTime = SystemAPI.Time.DeltaTime;

        foreach (var (aiState, patrolState, aggro, target, attackState, perception, entity) in SystemAPI.Query<
                     RefRW<EnemyState>,
                     RefRW<EnemyPatrolState>,
                     RefRW<EnemyAggroState>,
                     RefRW<EnemyTargetPosition>,
                     RefRW<EnemyAttackState>,
                     RefRO<EnemyPerceptionState>>()
                     .WithEntityAccess()
                     .WithNone<EnemyInactive>())
        {
            if (em.IsComponentEnabled<EnemyStateFrameLock>(entity) || aiState.ValueRO.Value != EnemyAIState.Patrol)
                continue;

            LocalTransform transform = em.GetComponentData<LocalTransform>(entity);
            EnemyPatrolSettings settings = em.GetComponentData<EnemyPatrolSettings>(entity);
            EnemyAI ai = em.GetComponentData<EnemyAI>(entity);
            DynamicBuffer<EnemyPatrolPoint> patrolPoints = em.GetBuffer<EnemyPatrolPoint>(entity);

            float3 posXZ = EnemyBrainUtility.Flatten(transform.Position);
            bool waitingNow = patrolState.ValueRO.IsWaiting;

            if (patrolPoints.Length > 0)
            {
                int index = patrolState.ValueRO.CurrentIndex;
                float3 patrolPoint = patrolPoints[index].Position;
                float patrolDist = math.distance(posXZ, EnemyBrainUtility.Flatten(patrolPoint));

                if (waitingNow)
                {
                    patrolState.ValueRW.WaitTimer += deltaTime;

                    if (patrolState.ValueRO.WaitTimer >= patrolState.ValueRO.CurrentWaitDuration)
                    {
                        patrolState.ValueRW.IsWaiting = false;
                        patrolState.ValueRW.WaitTimer = 0f;
                        patrolState.ValueRW.CurrentIndex = (index + 1) % patrolPoints.Length;
                        waitingNow = false;
                    }
                }

                if (!waitingNow)
                {
                    target.ValueRW.Value = patrolPoint;

                    if (patrolDist <= settings.ReachDistance)
                    {
                        patrolState.ValueRW.IsWaiting = true;
                        patrolState.ValueRW.CurrentWaitDuration =
                            settings.RandomPatrol
                                ? Unity.Mathematics.Random.CreateFromIndex((uint)(index + 1) * 1234)
                                    .NextFloat(settings.MinWaitTime, settings.MaxWaitTime)
                                : settings.MinWaitTime;
                    }
                }
            }

            if (perception.ValueRO.HasTarget &&
                perception.ValueRO.InsideVisionCone &&
                perception.ValueRO.HasValidLos &&
                perception.ValueRO.DistToTarget <= ai.VisionRange)
            {
                aggro.ValueRW.Timer += deltaTime;

                if (aggro.ValueRO.Timer > ai.AggroConfirmTime)
                {
                    aiState.ValueRW.Value = EnemyAIState.Chase;
                    attackState.ValueRW.Timer = 0f;
                    em.SetComponentEnabled<EnemyStateFrameLock>(entity, true);
                }
            }
            else
            {
                aggro.ValueRW.Timer = 0f;
            }
        }
    }
}

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(EnemyPatrolStateSystem))]
public partial struct EnemyChaseStateSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var em = state.EntityManager;

        foreach (var (aiState, lastKnown, target, attackState, perception, entity) in SystemAPI.Query<
                     RefRW<EnemyState>,
                     RefRW<EnemyLastKnownPosition>,
                     RefRW<EnemyTargetPosition>,
                     RefRW<EnemyAttackState>,
                     RefRO<EnemyPerceptionState>>()
                     .WithEntityAccess()
                     .WithNone<EnemyInactive>())
        {
            if (em.IsComponentEnabled<EnemyStateFrameLock>(entity) || aiState.ValueRO.Value != EnemyAIState.Chase)
                continue;

            LocalTransform transform = em.GetComponentData<LocalTransform>(entity);
            EnemyAI ai = em.GetComponentData<EnemyAI>(entity);

            if (!perception.ValueRO.HasTarget)
            {
                aiState.ValueRW.Value = EnemyAIState.Return;
                attackState.ValueRW.Timer = 0f;
                em.SetComponentEnabled<EnemyStateFrameLock>(entity, true);
                continue;
            }

            lastKnown.ValueRW.Value = perception.ValueRO.TargetPosition;

            float3 desiredPos = EnemyBrainUtility.GetDesiredCombatPosition(
                transform.Position,
                perception.ValueRO.TargetPosition,
                perception.ValueRO.PreferredCombatDistance
            );

            target.ValueRW.Value = desiredPos;

            if (perception.ValueRO.HasValidLos &&
                perception.ValueRO.DistToTarget <= perception.ValueRO.AttackEnterDistance)
            {
                aiState.ValueRW.Value = EnemyAIState.Attack;
                attackState.ValueRW.Timer = 0f;
                em.SetComponentEnabled<EnemyStateFrameLock>(entity, true);
            }
            else if (perception.ValueRO.DistToTarget > ai.LoseAggroRadius)
            {
                aiState.ValueRW.Value = EnemyAIState.Return;
                attackState.ValueRW.Timer = 0f;
                em.SetComponentEnabled<EnemyStateFrameLock>(entity, true);
            }
        }
    }
}

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(EnemyChaseStateSystem))]
public partial struct EnemyAttackStateSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var em = state.EntityManager;
        float deltaTime = SystemAPI.Time.DeltaTime;

        foreach (var (aiState, lastKnown, target, attackState, perception, entity) in SystemAPI.Query<
                     RefRW<EnemyState>,
                     RefRW<EnemyLastKnownPosition>,
                     RefRW<EnemyTargetPosition>,
                     RefRW<EnemyAttackState>,
                     RefRO<EnemyPerceptionState>>()
                     .WithEntityAccess()
                     .WithNone<EnemyInactive>())
        {
            if (em.IsComponentEnabled<EnemyStateFrameLock>(entity) || aiState.ValueRO.Value != EnemyAIState.Attack)
                continue;

            LocalTransform transform = em.GetComponentData<LocalTransform>(entity);
            EnemyAI ai = em.GetComponentData<EnemyAI>(entity);

            if (!perception.ValueRO.HasTarget)
            {
                aiState.ValueRW.Value = EnemyAIState.Return;
                var nextAttackState = attackState.ValueRO;
                EnemyBrainUtility.ResetAttack(ref nextAttackState, true);
                attackState.ValueRW = nextAttackState;
                em.SetComponentEnabled<EnemyStateFrameLock>(entity, true);
                continue;
            }

            lastKnown.ValueRW.Value = perception.ValueRO.TargetPosition;

            float3 desiredPos = EnemyBrainUtility.GetDesiredCombatPosition(
                transform.Position,
                perception.ValueRO.TargetPosition,
                perception.ValueRO.PreferredCombatDistance
            );

            if (perception.ValueRO.DistToTarget > perception.ValueRO.PreferredCombatDistance + ai.AttackMoveGoalTolerance ||
                perception.ValueRO.DistToTarget < perception.ValueRO.RetreatDistance)
                target.ValueRW.Value = desiredPos;
            else
                target.ValueRW.Value = transform.Position;

            if (!perception.ValueRO.HasValidLos)
                attackState.ValueRW.Timer += deltaTime;
            else
                attackState.ValueRW.Timer = 0f;

            if (attackState.ValueRO.Timer > ai.LostSightGraceTime ||
                perception.ValueRO.DistToTarget > perception.ValueRO.AttackExitDistance)
            {
                aiState.ValueRW.Value = EnemyAIState.Chase;
                target.ValueRW.Value = desiredPos;
                attackState.ValueRW.DoAttack = false;
                attackState.ValueRW.Timer = 0f;
                attackState.ValueRW.Type = EnemyAttackType.None;
                em.SetComponentEnabled<EnemyStateFrameLock>(entity, true);
                continue;
            }

            if (perception.ValueRO.DistToTarget <= ai.AttackRange &&
                attackState.ValueRO.Cooldown <= 0f)
            {
                attackState.ValueRW.Cooldown = ai.AttackCooldown;
                attackState.ValueRW.Type = ai.AttackType;
                attackState.ValueRW.DoAttack = true;
            }
        }
    }
}

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(EnemyAttackStateSystem))]
public partial struct EnemyReturnStateSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var em = state.EntityManager;

        foreach (var (aiState, aggro, lastKnown, target, attackState, perception, entity) in SystemAPI.Query<
                     RefRW<EnemyState>,
                     RefRW<EnemyAggroState>,
                     RefRW<EnemyLastKnownPosition>,
                     RefRW<EnemyTargetPosition>,
                     RefRW<EnemyAttackState>,
                     RefRO<EnemyPerceptionState>>()
                     .WithEntityAccess()
                     .WithNone<EnemyInactive>())
        {
            if (em.IsComponentEnabled<EnemyStateFrameLock>(entity) || aiState.ValueRO.Value != EnemyAIState.Return)
                continue;

            LocalTransform transform = em.GetComponentData<LocalTransform>(entity);
            EnemyAI ai = em.GetComponentData<EnemyAI>(entity);

            if (perception.ValueRO.HasTarget)
            {
                lastKnown.ValueRW.Value = perception.ValueRO.TargetPosition;

                float3 desiredPos = EnemyBrainUtility.GetDesiredCombatPosition(
                    transform.Position,
                    perception.ValueRO.TargetPosition,
                    perception.ValueRO.PreferredCombatDistance
                );

                target.ValueRW.Value = desiredPos;

                if (perception.ValueRO.InsideVisionCone &&
                    perception.ValueRO.HasValidLos &&
                    perception.ValueRO.DistToTarget <= perception.ValueRO.AttackEnterDistance)
                {
                    aiState.ValueRW.Value = EnemyAIState.Attack;
                    attackState.ValueRW.Timer = 0f;
                    em.SetComponentEnabled<EnemyStateFrameLock>(entity, true);
                    continue;
                }

                if (perception.ValueRO.DistToTarget <= ai.LoseAggroRadius)
                {
                    aiState.ValueRW.Value = EnemyAIState.Chase;
                    attackState.ValueRW.Timer = 0f;
                    em.SetComponentEnabled<EnemyStateFrameLock>(entity, true);
                    continue;
                }
            }

            float distToReturn = math.distance(
                EnemyBrainUtility.Flatten(transform.Position),
                EnemyBrainUtility.Flatten(lastKnown.ValueRO.Value)
            );

            target.ValueRW.Value = lastKnown.ValueRO.Value;

            if (distToReturn < ai.ReturnReachDistance)
            {
                aiState.ValueRW.Value = EnemyAIState.Patrol;
                aggro.ValueRW.Timer = 0f;
                attackState.ValueRW.Timer = 0f;
                attackState.ValueRW.DoAttack = false;
                attackState.ValueRW.Type = EnemyAttackType.None;
                em.SetComponentEnabled<EnemyStateFrameLock>(entity, true);
            }
        }
    }
}

internal static class EnemyBrainUtility
{
    public static EnemyPerceptionState BuildPerception(
        in EnemyAI ai,
        in LocalTransform transform,
        in EnemyTarget enemyTarget,
        in EnemyHasLineOfSight lineOfSight,
        ComponentLookup<LocalTransform> transformLookup,
        ComponentLookup<PlayerDead> deadLookup)
    {
        bool hasTarget = false;
        float3 targetPos = default;

        if (enemyTarget.Value != Entity.Null &&
            transformLookup.HasComponent(enemyTarget.Value) &&
            !deadLookup.HasComponent(enemyTarget.Value))
        {
            hasTarget = true;
            targetPos = transformLookup[enemyTarget.Value].Position;
        }

        float3 posXZ = Flatten(transform.Position);
        float3 targetXZ = Flatten(targetPos);
        float3 flatToTarget = hasTarget ? targetXZ - posXZ : float3.zero;
        float distToTarget = hasTarget ? math.length(flatToTarget) : 9999f;
        bool hasValidLos = !ai.RequireLOS || lineOfSight.Value;
        bool insideVisionCone = true;

        if (hasTarget && ai.VisionAngle < 360f)
        {
            float3 forward = math.mul(transform.Rotation, new float3(0f, 0f, 1f));
            float3 forwardXZ = math.normalizesafe(new float3(forward.x, 0f, forward.z), new float3(0f, 0f, 1f));
            float3 dirToTarget = math.normalizesafe(flatToTarget, new float3(0f, 0f, 1f));
            float minDot = math.cos(math.radians(ai.VisionAngle * 0.5f));
            insideVisionCone = math.dot(forwardXZ, dirToTarget) >= minDot;
        }

        float preferredCombatDistance = ai.PreferredCombatDistance > 0f
            ? ai.PreferredCombatDistance
            : ai.AttackRange * ai.StopDistanceMultiplier;
        float retreatDistance = ai.RetreatDistance > 0f
            ? ai.RetreatDistance
            : preferredCombatDistance * 0.7f;
        float attackEnterDistance = ai.ReengageDistance > 0f
            ? ai.ReengageDistance
            : math.max(0f, ai.AttackRange - ai.AttackEnterOffset);

        return new EnemyPerceptionState
        {
            HasTarget = hasTarget,
            HasValidLos = hasValidLos,
            InsideVisionCone = insideVisionCone,
            DistToTarget = distToTarget,
            TargetPosition = targetPos,
            PreferredCombatDistance = preferredCombatDistance,
            RetreatDistance = retreatDistance,
            AttackEnterDistance = attackEnterDistance,
            AttackExitDistance = ai.AttackRange + ai.AttackExitOffset
        };
    }

    public static float3 Flatten(float3 value)
    {
        value.y = 0f;
        return value;
    }

    public static float3 GetDesiredCombatPosition(float3 enemyPos, float3 targetPos, float preferredCombatDistance)
    {
        float3 dir = math.normalizesafe(Flatten(targetPos) - Flatten(enemyPos));
        return targetPos - dir * preferredCombatDistance;
    }

    public static void ResetAttack(ref EnemyAttackState attackState, bool resetCooldown)
    {
        attackState.DoAttack = false;
        attackState.Timer = 0f;
        attackState.Type = EnemyAttackType.None;

        if (resetCooldown)
            attackState.Cooldown = 0f;
    }
}
