using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
[WithNone(typeof(EnemyInactive))]
public partial struct EnemyAIJob : IJobEntity
{
    public float DeltaTime;

    [ReadOnly] public ComponentLookup<LocalTransform> TransformLookup;
    [ReadOnly] public ComponentLookup<PlayerTag> PlayerTagLookup;

    void Execute(
        ref EnemyState aiState,
        ref EnemyPatrolState patrolState,
        ref EnemyAggroState aggro,
        ref EnemyLastKnownPosition lastKnown,
        ref EnemyTargetPosition target,
        ref EnemyAttackState attackState,

        in EnemyAI ai,
        in EnemyPatrolSettings settings,
        in LocalTransform transform,
        in EnemyTarget enemyTarget,
        in EnemyHasLineOfSight lineOfSight,
        in DynamicBuffer<EnemyPatrolPoint> patrolPoints
    )
    {
        float3 pos = transform.Position;

        float3 posXZ = pos;
        posXZ.y = 0f;

        // ================= TARGET =================
        bool hasTarget = false;
        float3 playerPos = default;

        if (enemyTarget.Value != Entity.Null &&
            TransformLookup.HasComponent(enemyTarget.Value) &&
            PlayerTagLookup.HasComponent(enemyTarget.Value))
        {
            hasTarget = true;
            playerPos = TransformLookup[enemyTarget.Value].Position;
        }

        float3 playerXZ = playerPos;
        playerXZ.y = 0f;

        float3 flatToPlayer = hasTarget ? (playerXZ - posXZ) : float3.zero;
        float distToPlayer = hasTarget ? math.length(flatToPlayer) : 9999f;

        bool hasValidLos = !ai.RequireLOS || lineOfSight.Value;

        float attackEnterDistance = math.max(0f, ai.AttackRange - ai.AttackEnterOffset);
        float attackExitDistance = ai.AttackRange + ai.AttackExitOffset;

        // ================= COOLDOWN =================
        if (attackState.Cooldown > 0f)
            attackState.Cooldown -= DeltaTime;

        // ================= PATROL =================
        if (aiState.Value == EnemyAIState.Patrol)
        {
            bool waitingNow = patrolState.IsWaiting;

            if (patrolPoints.Length > 0)
            {
                int index = patrolState.CurrentIndex;
                float3 patrolPoint = patrolPoints[index].Position;

                float3 patrolXZ = patrolPoint;
                patrolXZ.y = 0f;

                float patrolDist = math.distance(posXZ, patrolXZ);

                if (waitingNow)
                {
                    patrolState.WaitTimer += DeltaTime;

                    if (patrolState.WaitTimer >= patrolState.CurrentWaitDuration)
                    {
                        patrolState.IsWaiting = false;
                        patrolState.WaitTimer = 0f;
                        patrolState.CurrentIndex = (index + 1) % patrolPoints.Length;
                        waitingNow = false;
                    }
                }

                if (!waitingNow)
                {
                    target.Value = patrolPoint;

                    if (patrolDist <= settings.ReachDistance)
                    {
                        patrolState.IsWaiting = true;
                        patrolState.CurrentWaitDuration =
                            settings.RandomPatrol
                                ? Unity.Mathematics.Random.CreateFromIndex((uint)(index + 1) * 1234)
                                    .NextFloat(settings.MinWaitTime, settings.MaxWaitTime)
                                : settings.MinWaitTime;
                    }
                }
            }

            if (hasTarget && hasValidLos && distToPlayer <= ai.VisionRange)
            {
                aggro.Timer += DeltaTime;

                if (aggro.Timer > 0.3f)
                {
                    aiState.Value = EnemyAIState.Chase;
                    attackState.Timer = 0f;
                }
            }
            else
            {
                aggro.Timer = 0f;
            }

            return;
        }

        // ================= CHASE =================
        if (aiState.Value == EnemyAIState.Chase)
        {
            if (!hasTarget)
            {
                aiState.Value = EnemyAIState.Return;
                attackState.Timer = 0f;
                return;
            }

            lastKnown.Value = playerPos;

            float3 dir = math.normalizesafe(flatToPlayer);
            float stopDistance = ai.AttackRange * ai.StopDistanceMultiplier;
            float3 desiredPos = playerPos - dir * stopDistance;

            target.Value = desiredPos;

            if (hasValidLos && distToPlayer <= attackEnterDistance)
            {
                aiState.Value = EnemyAIState.Attack;
                attackState.Timer = 0f;
            }
            else if (distToPlayer > ai.LoseAggroRadius)
            {
                aiState.Value = EnemyAIState.Return;
                attackState.Timer = 0f;
            }

            return;
        }

        // ================= ATTACK =================
        if (aiState.Value == EnemyAIState.Attack)
        {
            if (!hasTarget)
            {
                aiState.Value = EnemyAIState.Return;
                attackState.DoAttack = false;
                attackState.Timer = 0f;
                return;
            }

            lastKnown.Value = playerPos;

            float3 dir = math.normalizesafe(flatToPlayer);
            float stopDistance = ai.AttackRange * ai.StopDistanceMultiplier;
            float3 desiredPos = playerPos - dir * stopDistance;

            if (distToPlayer > stopDistance + 0.15f)
                target.Value = desiredPos;
            else
                target.Value = pos;

            if (!hasValidLos)
                attackState.Timer += DeltaTime;
            else
                attackState.Timer = 0f;

            if (attackState.Timer > 0.25f || distToPlayer > attackExitDistance)
            {
                aiState.Value = EnemyAIState.Chase;
                target.Value = desiredPos;
                attackState.DoAttack = false;
                attackState.Timer = 0f;
                return;
            }

            if (distToPlayer <= ai.AttackRange && attackState.Cooldown <= 0f)
            {
                attackState.Cooldown = ai.AttackCooldown;
                attackState.DoAttack = true;
            }

            return;
        }

        // ================= RETURN =================
        if (aiState.Value == EnemyAIState.Return)
        {
            if (hasTarget)
            {
                lastKnown.Value = playerPos;

                float3 dir = math.normalizesafe(flatToPlayer);
                float stopDistance = ai.AttackRange * ai.StopDistanceMultiplier;
                float3 desiredPos = playerPos - dir * stopDistance;

                target.Value = desiredPos;

                if (hasValidLos && distToPlayer <= attackEnterDistance)
                {
                    aiState.Value = EnemyAIState.Attack;
                    attackState.Timer = 0f;
                    return;
                }

                if (distToPlayer <= ai.LoseAggroRadius)
                {
                    aiState.Value = EnemyAIState.Chase;
                    attackState.Timer = 0f;
                    return;
                }
            }

            float3 returnPos = lastKnown.Value;

            float3 returnXZ = returnPos;
            returnXZ.y = 0f;

            float distToReturn = math.distance(posXZ, returnXZ);

            target.Value = returnPos;

            if (distToReturn < 1f)
            {
                aiState.Value = EnemyAIState.Patrol;
                aggro.Timer = 0f;
                attackState.Timer = 0f;
                attackState.DoAttack = false;
            }
        }
    }
}
