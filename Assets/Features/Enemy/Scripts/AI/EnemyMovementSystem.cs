using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public partial struct EnemyMovementSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        float dt = SystemAPI.Time.DeltaTime;

        foreach (var (transform, target, ai) in SystemAPI.Query<
            RefRW<LocalTransform>,
            RefRO<EnemyTargetPosition>,
            RefRO<EnemyAI>>())
        {
            float3 pos = transform.ValueRO.Position;
            float3 targetPos = target.ValueRO.Value;

            float3 toTarget = targetPos - pos;
            float dist = math.length(toTarget);

            if (dist < 0.05f)
                continue;

            float3 dir = toTarget / dist;

            pos += dir * ai.ValueRO.MoveSpeed * dt;

            transform.ValueRW.Position = pos;
        }
    }
}
