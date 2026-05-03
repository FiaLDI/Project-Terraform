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

        foreach (var (transform, target, ai, aggroSettings, aggroBuffer) in SystemAPI.Query<
                     RefRO<LocalTransform>,
                     RefRW<EnemyTarget>,
                     RefRO<EnemyAI>,
                     RefRO<EnemyAggroSettings>,
                     DynamicBuffer<EnemyAggroElement>>()
                     .WithNone<EnemyInactive>())
        {
            float3 pos = transform.ValueRO.Position;
            float loseDistance = math.max(ai.ValueRO.LoseAggroRadius, aggroSettings.ValueRO.LoseDistance);
            Entity currentTarget = target.ValueRW.Value;

            bool currentValid = TryBuildCandidate(
                currentTarget,
                pos,
                ai.ValueRO,
                aggroSettings.ValueRO,
                aggroBuffer,
                true,
                loseDistance,
                out float currentScore);

            Entity bestTarget = currentValid ? currentTarget : Entity.Null;
            float bestScore = currentValid ? currentScore : float.MinValue;

            var nearby = PlayerSpatialGrid.GetNearby(pos);

            for (int i = 0; i < nearby.Count; i++)
            {
                int netId = nearby[i];
                if (!PlayerRegistryECS.TryGet(netId, out var playerEntity))
                    continue;

                if (!TryBuildCandidate(
                        playerEntity,
                        pos,
                        ai.ValueRO,
                        aggroSettings.ValueRO,
                        aggroBuffer,
                        playerEntity == currentTarget,
                        loseDistance,
                        out float challengerScore))
                    continue;

                if (challengerScore > bestScore)
                {
                    bestScore = challengerScore;
                    bestTarget = playerEntity;
                }
            }

            if (currentValid && bestTarget != Entity.Null && bestTarget != currentTarget)
            {
                float switchThreshold = math.max(1f, aggroSettings.ValueRO.TargetSwitchThreshold);
                if (bestScore < currentScore * switchThreshold)
                    bestTarget = currentTarget;
            }

            target.ValueRW.Value = bestTarget;
        }
    }

    private bool TryBuildCandidate(
        Entity candidate,
        float3 enemyPos,
        in EnemyAI ai,
        in EnemyAggroSettings aggroSettings,
        DynamicBuffer<EnemyAggroElement> aggroBuffer,
        bool isCurrentTarget,
        float loseDistance,
        out float score)
    {
        score = float.MinValue;

        if (candidate == Entity.Null ||
            !transformLookup.HasComponent(candidate) ||
            deadLookup.HasComponent(candidate))
            return false;

        float3 playerPos = transformLookup[candidate].Position;
        float distance = math.distance(enemyPos, playerPos);
        float threat = GetThreat(aggroBuffer, candidate);
        float distanceLimit = isCurrentTarget
            ? loseDistance
            : threat > 0f
                ? loseDistance
                : ai.AggroRadius;

        if (distance > distanceLimit)
            return false;

        float proximityScore = math.max(0f, ai.AggroRadius - distance);
        score = threat + proximityScore;

        if (isCurrentTarget)
            score += math.max(0f, aggroSettings.CurrentTargetBias);

        return true;
    }

    private static float GetThreat(DynamicBuffer<EnemyAggroElement> aggroBuffer, Entity candidate)
    {
        for (int i = 0; i < aggroBuffer.Length; i++)
        {
            if (aggroBuffer[i].Target == candidate)
                return aggroBuffer[i].Value;
        }

        return 0f;
    }
}
