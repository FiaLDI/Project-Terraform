using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(EnemyAISystem))]
public partial struct EnemyTargetingSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (transform, target, aggroBuffer, ai, los) in SystemAPI.Query<
            RefRO<LocalTransform>,
            RefRW<EnemyTarget>,
            DynamicBuffer<EnemyAggroElement>,
            RefRO<EnemyAI>,
            RefRO<EnemyHasLineOfSight>>())
        {
            float3 pos = transform.ValueRO.Position;

            float bestScore = float.MinValue;
            Entity bestTarget = Entity.Null;

            for (int i = 0; i < aggroBuffer.Length; i++)
            {
                var entry = aggroBuffer[i];

                if (!SystemAPI.HasComponent<LocalTransform>(entry.Target))
                    continue;

                float3 targetPos =
                    SystemAPI.GetComponent<LocalTransform>(entry.Target).Position;

                float dist = math.distance(pos, targetPos);

                if (dist > ai.ValueRO.LoseAggroRadius)
                    continue;

                // ================= LOS =================
                if (ai.ValueRO.RequireLOS && !los.ValueRO.Value)
                    continue;

                // ================= SCORE =================
                float score = entry.Value - dist * 0.5f;

                if (score > bestScore)
                {
                    bestScore = score;
                    bestTarget = entry.Target;
                }
            }
            
            if (aggroBuffer.Length == 0)
{
    // 👇 простой поиск игрока рядом
    foreach (var (playerTransform, playerEntity) in SystemAPI
        .Query<RefRO<LocalTransform>>()
        .WithAll<PlayerTag>()
        .WithEntityAccess())
    {
        float dist = math.distance(pos, playerTransform.ValueRO.Position);

        if (dist <= ai.ValueRO.AggroRadius)
        {
            bestTarget = playerEntity;
            break;
        }
    }
}

            target.ValueRW.Value = bestTarget;
        }
    }
}
