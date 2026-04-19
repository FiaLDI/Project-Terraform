using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct EnemyTargetingSystem : ISystem
{
    private ComponentLookup<LocalTransform> transformLookup;
    private ComponentLookup<PlayerDead> deadLookup;

    public void OnCreate(ref SystemState state)
    {
        transformLookup = state.GetComponentLookup<LocalTransform>(true);
        deadLookup = state.GetComponentLookup<PlayerDead>(true);
    }

    public void OnUpdate(ref SystemState state)
    {
        transformLookup.Update(ref state);
        deadLookup.Update(ref state);

        foreach (var (transform, target, ai) in SystemAPI.Query<
                     RefRO<LocalTransform>,
                     RefRW<EnemyTarget>,
                     RefRO<EnemyAI>>())
        {
            float3 pos = transform.ValueRO.Position;

            Entity bestTarget = Entity.Null;
            float bestDist = float.MaxValue;

            // 🔥 spatial grid
            var nearby = PlayerSpatialGrid.GetNearby(pos);

            for (int i = 0; i < nearby.Count; i++)
            {
                int netId = nearby[i];
                if (!PlayerRegistryECS.TryGet(netId, out var p))
                    continue;

                if (!transformLookup.HasComponent(p))
                    continue;

                if (deadLookup.HasComponent(p))
                    continue;

                float3 pPos = transformLookup[p].Position;

                float dist = math.distance(pos, pPos);

                if (dist < ai.ValueRO.AggroRadius && dist < bestDist)
                {
                    bestDist = dist;
                    bestTarget = p;
                }
            }

            target.ValueRW.Value = bestTarget;
        }
    }
}
