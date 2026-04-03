using Unity.Burst;
using Unity.Entities;
using Unity.Collections;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(EnemyDespawnSystem))]
public partial struct EnemyDespawnCleanupSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (_, entity) in SystemAPI
                     .Query<RefRO<EnemyMarkedForDespawn>>()
                     .WithEntityAccess())
        {
            
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}
