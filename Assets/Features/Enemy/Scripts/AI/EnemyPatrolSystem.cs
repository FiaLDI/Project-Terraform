using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public partial struct EnemyPatrolSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (enemyState, transform, target) in
                 SystemAPI.Query<
                     RefRO<EnemyState>,
                     RefRO<LocalTransform>,
                     RefRW<EnemyTargetPosition>>()
                 .WithAll<EnemyTag>())
        {
            if (enemyState.ValueRO.Value != EnemyAIState.Patrol)
                continue;

            // ПРОСТО ИДТИ ВПЕРЁД
            float3 forward = new float3(0, 0, 1);

            target.ValueRW.Value =
                transform.ValueRO.Position + forward * 3f;
        }
    }
}
