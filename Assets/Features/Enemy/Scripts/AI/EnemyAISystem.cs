using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public partial struct EnemyAISystem : ISystem
{
    private EntityQuery enemyQuery;
    private EntityQuery playerQuery;

    public void OnCreate(ref SystemState state)
    {
        enemyQuery = state.GetEntityQuery(
            ComponentType.ReadOnly<EnemyTag>(),
            ComponentType.ReadOnly<LocalTransform>(),
            ComponentType.ReadWrite<EnemyTargetPosition>(),
            ComponentType.ReadOnly<EnemyAI>(),
            ComponentType.ReadWrite<EnemyState>(),
            ComponentType.ReadWrite<EnemyPatrolState>(),
            ComponentType.ReadOnly<EnemyPatrolSettings>(),
            ComponentType.ReadWrite<EnemyBlocked>(),
            ComponentType.ReadOnly<EnemyPatrolPoint>()
        );

        playerQuery = state.GetEntityQuery(
            ComponentType.ReadOnly<PlayerTag>(),
            ComponentType.ReadOnly<LocalTransform>()
        );
    }

    public void OnUpdate(ref SystemState state)
    {
        float dt = SystemAPI.Time.DeltaTime;

        // ===== ИЩЕМ ИГРОКА =====
        bool hasPlayer = false;
        float3 playerPos = default;

        if (!playerQuery.IsEmptyIgnoreFilter)
        {
            var playerTransforms =
                playerQuery.ToComponentDataArray<LocalTransform>(state.WorldUpdateAllocator);

            playerPos = playerTransforms[0].Position;
            hasPlayer = true;
        }

        // ===== AI ЦИКЛ =====
        var entities = enemyQuery.ToEntityArray(state.WorldUpdateAllocator);
        var transforms = enemyQuery.ToComponentDataArray<LocalTransform>(state.WorldUpdateAllocator);
        var ais = enemyQuery.ToComponentDataArray<EnemyAI>(state.WorldUpdateAllocator);
        var states = enemyQuery.ToComponentDataArray<EnemyState>(state.WorldUpdateAllocator);
        var patrolStates = enemyQuery.ToComponentDataArray<EnemyPatrolState>(state.WorldUpdateAllocator);
        var patrolSettings = enemyQuery.ToComponentDataArray<EnemyPatrolSettings>(state.WorldUpdateAllocator);
        var blockedArr = enemyQuery.ToComponentDataArray<EnemyBlocked>(state.WorldUpdateAllocator);

        for (int i = 0; i < entities.Length; i++)
        {
            var entity = entities[i];
            var pos = transforms[i].Position;

            var ai = ais[i];
            var aiState = states[i];
            var patrolState = patrolStates[i];
            var settings = patrolSettings[i];
            var blocked = blockedArr[i];

            var patrolPoints =
                state.EntityManager.GetBuffer<EnemyPatrolPoint>(entity);

            // ========================= PATROL =========================
            if (aiState.Value == EnemyAIState.Patrol)
            {
                if (hasPlayer &&
                    math.distance(pos, playerPos) <= ai.AggroRadius)
                {
                    aiState.Value = EnemyAIState.Chase;
                }
                else if (patrolPoints.Length > 0)
                {
                    if (patrolState.IsWaiting)
                    {
                        patrolState.WaitTimer += dt;
                        if (patrolState.WaitTimer >= patrolState.CurrentWaitDuration)
                        {
                            patrolState.IsWaiting = false;
                            patrolState.WaitTimer = 0f;
                        }
                    }
                    else
                    {
                        int index = patrolState.CurrentIndex;
                        float3 patrolPoint = patrolPoints[index].Position;

                        float3 target = patrolPoint;
                        target.y = pos.y;

                        state.EntityManager.SetComponentData(
                            entity,
                            new EnemyTargetPosition { Value = target });

                        float2 posXZ = new(pos.x, pos.z);
                        float2 targetXZ = new(patrolPoint.x, patrolPoint.z);

                        if (math.distance(posXZ, targetXZ) <= settings.ReachDistance
                            || blocked.Value)
                        {
                            blocked.Value = false;
                            patrolState.IsWaiting = true;
                            patrolState.CurrentWaitDuration =
                                UnityEngine.Random.Range(settings.MinWaitTime, settings.MaxWaitTime);

                            patrolState.CurrentIndex =
                                settings.RandomPatrol
                                    ? UnityEngine.Random.Range(0, patrolPoints.Length)
                                    : (index + 1) % patrolPoints.Length;
                        }
                    }
                }
            }

            // ========================= CHASE =========================
            else if (aiState.Value == EnemyAIState.Chase)
            {
                if (!hasPlayer)
                {
                    aiState.Value = EnemyAIState.Return;
                }
                else
                {
                    state.EntityManager.SetComponentData(
                        entity,
                        new EnemyTargetPosition { Value = playerPos });

                    if (math.distance(pos, playerPos) > ai.LoseAggroRadius)
                        aiState.Value = EnemyAIState.Return;
                }
            }

            // ========================= RETURN =========================
            else if (aiState.Value == EnemyAIState.Return && patrolPoints.Length > 0)
            {
                float minDist = float.MaxValue;
                int nearest = 0;

                for (int p = 0; p < patrolPoints.Length; p++)
                {
                    float d = math.distance(pos, patrolPoints[p].Position);
                    if (d < minDist)
                    {
                        minDist = d;
                        nearest = p;
                    }
                }

                patrolState.CurrentIndex = nearest;
                float3 ret = patrolPoints[nearest].Position;
                ret.y = pos.y;

                state.EntityManager.SetComponentData(
                    entity,
                    new EnemyTargetPosition { Value = ret });

                if (math.distance(
                        new float2(pos.x, pos.z),
                        new float2(ret.x, ret.z)) <= settings.ReachDistance)
                {
                    aiState.Value = EnemyAIState.Patrol;
                }
            }

            // ===== записываем изменения =====
            state.EntityManager.SetComponentData(entity, aiState);
            state.EntityManager.SetComponentData(entity, patrolState);
            state.EntityManager.SetComponentData(entity, blocked);
        }
    }
}
