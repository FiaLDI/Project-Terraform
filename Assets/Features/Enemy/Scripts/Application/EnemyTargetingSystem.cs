using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(EnemyLOSSystemECS))]
public partial struct EnemyTargetingSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (transform, target, aggroBuffer, ai) in SystemAPI.Query<
                     RefRO<LocalTransform>,
                     RefRW<EnemyTarget>,
                     DynamicBuffer<EnemyAggroElement>,
                     RefRO<EnemyAI>>())
        {
            float3 pos = transform.ValueRO.Position;
            float3 posXZ = pos;
            posXZ.y = 0f;

            Entity currentTarget = target.ValueRO.Value;
            Entity bestTarget = Entity.Null;
            float bestScore = float.MinValue;

            // 1) Приоритет: цели из aggro buffer
            for (int i = 0; i < aggroBuffer.Length; i++)
            {
                var entry = aggroBuffer[i];

                if (!SystemAPI.Exists(entry.Target) ||
                    !SystemAPI.HasComponent<LocalTransform>(entry.Target) ||
                    !SystemAPI.HasComponent<PlayerTag>(entry.Target))
                    continue;

                float3 targetPos = SystemAPI.GetComponent<LocalTransform>(entry.Target).Position;
                float3 targetXZ = targetPos;
                targetXZ.y = 0f;

                float dist = math.distance(posXZ, targetXZ);
                if (dist > ai.ValueRO.LoseAggroRadius)
                    continue;

                float score = entry.Value - dist * 0.5f;
                if (score > bestScore)
                {
                    bestScore = score;
                    bestTarget = entry.Target;
                }
            }

            // 2) Если текущая цель ещё валидна — держим её, чтобы не дёргать переключения
            if (bestTarget == Entity.Null &&
                currentTarget != Entity.Null &&
                SystemAPI.Exists(currentTarget) &&
                SystemAPI.HasComponent<LocalTransform>(currentTarget) &&
                SystemAPI.HasComponent<PlayerTag>(currentTarget))
            {
                float3 currentPos = SystemAPI.GetComponent<LocalTransform>(currentTarget).Position;
                float3 currentXZ = currentPos;
                currentXZ.y = 0f;

                float currentDist = math.distance(posXZ, currentXZ);
                if (currentDist <= ai.ValueRO.LoseAggroRadius)
                    bestTarget = currentTarget;
            }

            // 3) Если цели всё ещё нет — ищем ближайшего игрока в aggro radius
            if (bestTarget == Entity.Null)
            {
                float nearestDist = float.MaxValue;

                foreach (var (playerTransform, playerEntity) in SystemAPI
                         .Query<RefRO<LocalTransform>>()
                         .WithAll<PlayerTag>()
                         .WithEntityAccess())
                {
                    float3 playerPos = playerTransform.ValueRO.Position;
                    float3 playerXZ = playerPos;
                    playerXZ.y = 0f;

                    float dist = math.distance(posXZ, playerXZ);
                    if (dist > ai.ValueRO.AggroRadius || dist >= nearestDist)
                        continue;

                    nearestDist = dist;
                    bestTarget = playerEntity;
                }
            }

            target.ValueRW.Value = bestTarget;
        }
    }
}
