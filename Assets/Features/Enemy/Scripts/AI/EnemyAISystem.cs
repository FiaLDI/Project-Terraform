using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public partial struct EnemyAISystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        if (!SystemAPI.TryGetSingleton<PlayerTag>(out _))
            return;

        float3 playerPos =
            SystemAPI.GetSingleton<LocalTransform>().Position;

        foreach (var (ai, enemyState, transform, target) in
                 SystemAPI.Query<
                     RefRO<EnemyAI>,
                     RefRW<EnemyState>,
                     RefRO<LocalTransform>,
                     RefRW<EnemyTargetPosition>>()
                 .WithAll<EnemyTag>())
        {
            float dist =
                math.distance(transform.ValueRO.Position, playerPos);

            if (enemyState.ValueRO.Value == EnemyAIState.Patrol &&
                dist <= ai.ValueRO.AggroRadius)
            {
                enemyState.ValueRW.Value = EnemyAIState.Chase;
                target.ValueRW.Value = playerPos;
            }

            if (enemyState.ValueRO.Value == EnemyAIState.Chase)
            {
                target.ValueRW.Value = playerPos;
            }
        }
    }
}
