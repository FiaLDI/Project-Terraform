using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public partial struct EnemyAISystem : ISystem
{
    private EntityQuery playerQuery;

    public void OnCreate(ref SystemState state)
    {
        playerQuery = state.GetEntityQuery(
            ComponentType.ReadOnly<PlayerTag>(),
            ComponentType.ReadOnly<LocalTransform>()
        );
    }

    public void OnUpdate(ref SystemState state)
    {
        float dt = SystemAPI.Time.DeltaTime;

        bool hasPlayer = false;
        float3 playerPos = default;

        if (!playerQuery.IsEmptyIgnoreFilter)
        {
            var players = playerQuery.ToComponentDataArray<LocalTransform>(state.WorldUpdateAllocator);
            playerPos = players[0].Position;
            hasPlayer = true;
        }

        new EnemyAIJob
        {
            DeltaTime = dt,
            HasPlayer = hasPlayer,
            PlayerPosition = playerPos
        }.ScheduleParallel();
    }
}
