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

    [ReadOnly]
    public ComponentLookup<LocalTransform> TransformLookup;

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
        in DynamicBuffer<EnemyPatrolPoint> patrolPoints
    )
    {
        float3 pos = transform.Position;

        float3 posXZ = pos;
        posXZ.y = 0;

        // ================= TARGET =================
        bool hasTarget = false;
        float3 playerPos = default;

        if (enemyTarget.Value != Entity.Null &&
            TransformLookup.HasComponent(enemyTarget.Value))
        {
            hasTarget = true;
            playerPos = TransformLookup[enemyTarget.Value].Position;
        }

        float3 playerXZ = playerPos;
        playerXZ.y = 0;

        float3 flatToPlayer = hasTarget ? (playerXZ - posXZ) : float3.zero;
        float distToPlayer = hasTarget ? math.length(flatToPlayer) : 9999f;

        // ================= COOLDOWN =================
        if (attackState.Cooldown > 0f)
            attackState.Cooldown -= DeltaTime;

        // ================= PATROL =================
        if (aiState.Value == EnemyAIState.Patrol)
        {
            // 👉 ВСЕГДА патруль
            if (patrolPoints.Length > 0)
            {
                int index = patrolState.CurrentIndex;

                float3 patrolPoint = patrolPoints[index].Position;
                patrolPoint.y = pos.y;

                float3 patrolXZ = patrolPoint;
                patrolXZ.y = 0;

                float dist = math.distance(posXZ, patrolXZ);

                if (patrolState.IsWaiting)
                {
                    patrolState.WaitTimer += DeltaTime;

                    if (patrolState.WaitTimer >= patrolState.CurrentWaitDuration)
                    {
                        patrolState.IsWaiting = false;
                        patrolState.WaitTimer = 0f;

                        patrolState.CurrentIndex =
                            (index + 1) % patrolPoints.Length;
                    }

                    return;
                }

                target.Value = patrolPoint;

                if (dist <= settings.ReachDistance)
                {
                    patrolState.IsWaiting = true;

                    patrolState.CurrentWaitDuration =
                        settings.RandomPatrol
                            ? Unity.Mathematics.Random.CreateFromIndex((uint)(index + 1) * 1234)
                                .NextFloat(settings.MinWaitTime, settings.MaxWaitTime)
                            : settings.MinWaitTime;
                }
            }

            // 👉 если есть цель — пробуем агр
            if (hasTarget && distToPlayer <= ai.VisionRange)
            {
                aggro.Timer += DeltaTime;

                if (aggro.Timer > 0.3f)
                    aiState.Value = EnemyAIState.Chase;
            }
            else
            {
                aggro.Timer = 0f;
            }
        }

        // ================= CHASE =================
        else if (aiState.Value == EnemyAIState.Chase)
        {
            if (!hasTarget)
            {
                aiState.Value = EnemyAIState.Return;
                return;
            }

            lastKnown.Value = playerPos;

            float3 dir = math.normalizesafe(flatToPlayer);

            float stopDistance = ai.AttackRange * ai.StopDistanceMultiplier;

            float3 desiredPos = playerPos - dir * stopDistance;
            desiredPos.y = pos.y;

            target.Value = desiredPos;

            if (distToPlayer <= ai.AttackRange)
            {
                aiState.Value = EnemyAIState.Attack;
            }
            else if (distToPlayer > ai.LoseAggroRadius)
            {
                aiState.Value = EnemyAIState.Return;
            }
        }

        // ================= ATTACK =================
        else if (aiState.Value == EnemyAIState.Attack)
        {
            if (!hasTarget)
            {
                aiState.Value = EnemyAIState.Return;
                return;
            }

            if (attackState.Cooldown <= 0f)
            {
                attackState.Cooldown = ai.AttackCooldown;
                attackState.DoAttack = true;
            }

            if (distToPlayer > ai.AttackRange + 2f)
            {
                aiState.Value = EnemyAIState.Chase;
            }
        }

        // ================= RETURN =================
        else if (aiState.Value == EnemyAIState.Return)
        {
            float3 returnPos = lastKnown.Value;
            returnPos.y = pos.y;

            float3 returnXZ = returnPos;
            returnXZ.y = 0;

            float dist = math.distance(posXZ, returnXZ);

            target.Value = returnPos;

            if (dist < 1f)
            {
                aiState.Value = EnemyAIState.Patrol;
                aggro.Timer = 0f;
            }
        }
    }
}
