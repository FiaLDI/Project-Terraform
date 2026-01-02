using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

[BurstCompile]
public partial struct EnemyMoveToTargetSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        float dt = SystemAPI.Time.DeltaTime;

        foreach (var (transform, target, ai) in
                SystemAPI.Query<
                    RefRW<LocalTransform>,
                    RefRO<EnemyTargetPosition>,
                    RefRO<EnemyAI>>())
        {
            float3 dir = target.ValueRO.Value - transform.ValueRO.Position;

            if (math.lengthsq(dir) < 0.01f)
                continue;

            dir = math.normalize(dir);
            transform.ValueRW.Position += dir * ai.ValueRO.MoveSpeed * dt;
        }

    }
}
