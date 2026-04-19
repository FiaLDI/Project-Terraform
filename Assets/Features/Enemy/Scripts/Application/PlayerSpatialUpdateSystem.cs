using Unity.Entities;
using Unity.Transforms;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(EnemyTargetingSystem))]
public partial struct PlayerSpatialUpdateSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var em = state.EntityManager;

        PlayerSpatialGrid.Clear();

        var players = PlayerRegistryECS.All;

        for (int i = 0; i < players.Count; i++)
        {
            var entity = players[i];

            if (!em.Exists(entity))
                continue;

            if (!PlayerRegistryECS.TryGetNetId(entity, out int netId))
                continue;

            var pos = em.GetComponentData<LocalTransform>(entity).Position;

            PlayerSpatialGrid.Add(netId, pos);
        }
    }
}
