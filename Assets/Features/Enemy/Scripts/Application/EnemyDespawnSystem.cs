using Unity.Burst;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct EnemyDespawnSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (enemyTransform, despawn, entity) in SystemAPI.Query<
                     RefRO<LocalTransform>,
                     RefRO<EnemyDespawnDistance>>()
                 .WithAll<EnemyTag>()
                 .WithEntityAccess())
        {
            float3 enemyPos = enemyTransform.ValueRO.Position;

            bool shouldDespawn = true;

            foreach (var (playerTransform, _) in SystemAPI
                         .Query<RefRO<LocalTransform>>()
                         .WithAll<PlayerTag>()
                         .WithEntityAccess())
            {
                float dist = math.distance(enemyPos, playerTransform.ValueRO.Position);

                if (dist <= despawn.ValueRO.Value)
                {
                    shouldDespawn = false;
                    break;
                }
            }

            if (shouldDespawn)
            {
                if (!SystemAPI.HasComponent<EnemyMarkedForDespawn>(entity))
                {
                    ecb.AddComponent<EnemyMarkedForDespawn>(entity);
                }
            }
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}
