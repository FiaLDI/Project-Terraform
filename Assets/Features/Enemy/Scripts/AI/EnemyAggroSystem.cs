using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

[BurstCompile]
public partial struct EnemyAggroSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        // Берём всех игроков
        var playerQuery =
            SystemAPI.QueryBuilder()
                .WithAll<PlayerTag, LocalTransform>()
                .Build();

        if (playerQuery.IsEmpty)
            return;

        var players =
            playerQuery.ToComponentDataArray<LocalTransform>(
                state.WorldUpdateAllocator
            );

        foreach (var (ai, enemyState, enemyTransform, targetPos) in
                 SystemAPI.Query<
                     RefRO<EnemyAI>,
                     RefRW<EnemyState>,
                     RefRO<LocalTransform>,
                     RefRW<EnemyTargetPosition>>()
                     .WithAll<EnemyTag>())
        {
            float3 enemyPos = enemyTransform.ValueRO.Position;

            float closestDist = float.MaxValue;
            float3 closestPlayerPos = float3.zero;
            bool playerFound = false;

            // ищем ближайшего игрока
            for (int i = 0; i < players.Length; i++)
            {
                float dist = math.distance(enemyPos, players[i].Position);

                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestPlayerPos = players[i].Position;
                    playerFound = true;
                }
            }

            if (!playerFound)
                continue;

            switch (enemyState.ValueRW.Value)
            {
                case EnemyAIState.Patrol:
                    if (closestDist <= ai.ValueRO.AggroRadius)
                    {
                        enemyState.ValueRW.Value = EnemyAIState.Chase;
                        targetPos.ValueRW.Value = closestPlayerPos;
                    }
                    break;

                case EnemyAIState.Chase:
                    if (closestDist > ai.ValueRO.LoseAggroRadius)
                    {
                        enemyState.ValueRW.Value = EnemyAIState.Patrol;
                    }
                    else
                    {
                        targetPos.ValueRW.Value = closestPlayerPos;
                    }
                    break;
            }
        }
    }
}
