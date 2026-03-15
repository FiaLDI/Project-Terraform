using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public partial struct EnemyAIJob : IJobEntity
{
    public float DeltaTime;
    public bool HasPlayer;
    public float3 PlayerPosition;

    void Execute(
        ref EnemyState aiState,
        ref EnemyPatrolState patrolState,
        ref EnemyBlocked blocked,
        ref EnemyAggroState aggro,
        ref EnemyLastKnownPosition lastKnown,
        ref EnemyTargetPosition target,
        ref EnemyAttackState attackState,

        in EnemyAI ai,
        in EnemyPatrolSettings settings,
        in LocalTransform transform,
        in DynamicBuffer<EnemyPatrolPoint> patrolPoints
    )
    {
        float3 pos = transform.Position;

        // 👉 XZ позиция
        float3 posXZ = pos;
        posXZ.y = 0;

        // ================= COOLDOWN =================
        if (attackState.Cooldown > 0f)
            attackState.Cooldown -= DeltaTime;

        // ================= PATROL =================
        if (aiState.Value == EnemyAIState.Patrol)
        {
            // ---------- AGGRO CHECK ----------
            if (HasPlayer)
            {
                float3 playerXZ = PlayerPosition;
                playerXZ.y = 0;

                float distToPlayer = math.distance(posXZ, playerXZ);

                if (distToPlayer <= ai.AggroRadius)
                {
                    aggro.Timer += DeltaTime;

                    if (aggro.Timer > 0.5f)
                        aiState.Value = EnemyAIState.Chase;
                }
                else
                {
                    aggro.Timer = 0f;
                }
            }

            // ---------- PATROL ----------
            if (patrolPoints.Length > 0)
            {
                int index = patrolState.CurrentIndex;
                float3 patrolPoint = patrolPoints[index].Position;

                // 👉 делаем ту же высоту
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

                // 🔥 ВСЕГДА обновляем target
                target.Value = patrolPoint;

                if (dist <= settings.ReachDistance)
                {
                    patrolState.IsWaiting = true;
                    patrolState.CurrentWaitDuration = 1f;
                }
            }
        }

        // ================= CHASE =================
        else if (aiState.Value == EnemyAIState.Chase)
        {
            if (!HasPlayer)
            {
                aiState.Value = EnemyAIState.Return;
                return;
            }

            float3 playerXZ = PlayerPosition;
            playerXZ.y = 0;

            float dist = math.distance(posXZ, playerXZ);

            lastKnown.Value = PlayerPosition;

            float3 dir = math.normalizesafe(PlayerPosition - pos);

            float stopDistance = ai.AttackRange * 0.9f;

            float3 desiredPos =
                PlayerPosition - dir * stopDistance;

            desiredPos.y = pos.y;

            target.Value = desiredPos;

            if (dist <= ai.AttackRange)
            {
                aiState.Value = EnemyAIState.Attack;
            }
            else if (dist > ai.LoseAggroRadius)
            {
                aiState.Value = EnemyAIState.Return;
            }
        }

        // ================= ATTACK =================
        else if (aiState.Value == EnemyAIState.Attack)
        {
            if (!HasPlayer)
            {
                aiState.Value = EnemyAIState.Return;
                return;
            }

            float3 playerXZ = PlayerPosition;
            playerXZ.y = 0;

            float dist = math.distance(posXZ, playerXZ);

            float3 dir = math.normalizesafe(pos - PlayerPosition);

            float3 desiredPos =
                PlayerPosition + dir * ai.AttackRange;

            desiredPos.y = pos.y;

            target.Value = desiredPos;

            if (dist > ai.AttackRange + 0.5f)
            {
                aiState.Value = EnemyAIState.Chase;
                return;
            }

            if (attackState.Cooldown <= 0f)
            {
                attackState.Cooldown = ai.AttackCooldown;
                attackState.DoAttack = true;
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
