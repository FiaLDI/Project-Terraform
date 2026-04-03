using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct EnemyAISystem : ISystem
{
    private ComponentLookup<LocalTransform> transformLookup;

    public void OnCreate(ref SystemState state)
    {
        transformLookup = state.GetComponentLookup<LocalTransform>(true);
    }

    public void OnUpdate(ref SystemState state)
    {
        float dt = SystemAPI.Time.DeltaTime;

        transformLookup.Update(ref state);

        new EnemyAIJob
        {
            DeltaTime = dt,
            TransformLookup = transformLookup
        }.ScheduleParallel();
    }
}
