using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
[WithNone(typeof(EnemyInactive))]
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
        in DynamicBuffer<EnemyPatrolPoint> patrolPoints,
        in EnemyHasLineOfSight los
    )
    {
        float3 pos = transform.Position;

        float3 posXZ = pos;
        posXZ.y = 0;

        // ================= COOLDOWN =================
        if (attackState.Cooldown > 0f)
            attackState.Cooldown -= DeltaTime;

        // ================= PATROL =================
        if (aiState.Value == EnemyAIState.Patrol)
        {
            if (HasPlayer)
            {
                float3 toPlayer = PlayerPosition - pos;
                float dist = math.length(toPlayer);

                if (dist <= ai.VisionRange)
                {
                    float3 forward = math.forward(transform.Rotation);
                    float3 dir = math.normalizesafe(toPlayer);

                    float dot = math.dot(forward, dir);
                    float cosHalf = math.cos(math.radians(ai.VisionAngle * 0.5f));

                    if (dot >= cosHalf)
                    {
                        // 🔥 ВОТ СЮДА
                        if (!ai.RequireLOS || los.Value)
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
                    else
                    {
                        aggro.Timer = 0f;
                    }
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

                patrolPoint.y = pos.y;

                float3 patrolXZ = patrolPoint;
                patrolXZ.y = 0;

                float dist = math.distance(posXZ, patrolXZ);

                // 👉 если ждём
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

                    // ❗ важно — НЕ обновляем target пока ждём
                    return;
                }

                // 👉 двигаемся к точке
                target.Value = patrolPoint;

                // 👉 дошли
                if (dist <= settings.ReachDistance)
                {
                    patrolState.IsWaiting = true;

                    patrolState.CurrentWaitDuration =
                        settings.RandomPatrol
                            ? Unity.Mathematics.Random.CreateFromIndex(
                                (uint)(index + 1) * 1234
                            ).NextFloat(settings.MinWaitTime, settings.MaxWaitTime)
                            : settings.MinWaitTime;
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

            float stopDistance = ai.AttackRange * ai.StopDistanceMultiplier;

            float3 desiredPos = PlayerPosition - dir * stopDistance;
            desiredPos.y = pos.y;

            target.Value = desiredPos;

            float enterAttack = ai.AttackRange + ai.AttackEnterOffset;

            if (dist <= enterAttack)
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

            float exitAttack = ai.AttackRange + ai.AttackExitOffset;

            if (dist > exitAttack)
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
