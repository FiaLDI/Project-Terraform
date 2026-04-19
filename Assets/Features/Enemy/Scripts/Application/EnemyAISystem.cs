using Unity.Entities;
using Unity.Transforms;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(EnemyLOSSystemECS))]
public partial struct EnemyAISystem : ISystem
{
    private ComponentLookup<LocalTransform> transformLookup;
    private ComponentLookup<PlayerTag> playerTagLookup;
    private ComponentLookup<PlayerDead> deadLookup;
    
    public void OnCreate(ref SystemState state)
    {
        transformLookup = state.GetComponentLookup<LocalTransform>(true);
        playerTagLookup = state.GetComponentLookup<PlayerTag>(true);
        deadLookup = state.GetComponentLookup<PlayerDead>(true);
    }

    public void OnUpdate(ref SystemState state)
    {
        transformLookup.Update(ref state);
        playerTagLookup.Update(ref state);
        deadLookup.Update(ref state);

        new EnemyAIJob
        {
            DeltaTime = SystemAPI.Time.DeltaTime,
            TransformLookup = transformLookup,
            PlayerTagLookup = playerTagLookup,
            deadLookup = deadLookup
        }.ScheduleParallel();
    }
}
