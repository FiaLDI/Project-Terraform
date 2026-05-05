using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(EnemyTargetingSystem))]
[UpdateBefore(typeof(EnemyAISystem))]
public partial struct EnemyLOSSystemECS : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (transform, enemyTarget, ai, los) in SystemAPI.Query<
                     RefRO<LocalTransform>,
                     RefRO<EnemyTarget>,
                     RefRO<EnemyAI>,
                     RefRW<EnemyHasLineOfSight>>()
                     .WithNone<EnemyInactive>())
        {
            if (!ai.ValueRO.RequireLOS)
            {
                los.ValueRW.Value = true;
                continue;
            }

            Entity targetEntity = enemyTarget.ValueRO.Value;

            if (targetEntity == Entity.Null ||
                !SystemAPI.Exists(targetEntity) ||
                !SystemAPI.HasComponent<LocalTransform>(targetEntity) ||
                !SystemAPI.HasComponent<PlayerTag>(targetEntity))
            {
                los.ValueRW.Value = false;
                continue;
            }

            Vector3 origin = (Vector3)transform.ValueRO.Position + Vector3.up * 1.5f;
            Vector3 destination = (Vector3)SystemAPI.GetComponent<LocalTransform>(targetEntity).Position + Vector3.up * 1.0f;

            Vector3 dir = destination - origin;
            float dist = dir.magnitude;

            if (dist <= 0.001f)
            {
                los.ValueRW.Value = true;
                continue;
            }

            dir /= dist;

            float3 forward = math.mul(transform.ValueRO.Rotation, new float3(0f, 0f, 1f));
            float3 flatForward = math.normalizesafe(new float3(forward.x, 0f, forward.z), new float3(0f, 0f, 1f));
            float3 flatDir = math.normalizesafe(new float3(dir.x, 0f, dir.z), new float3(0f, 0f, 1f));
            float minDot = math.cos(math.radians(ai.ValueRO.VisionAngle * 0.5f));

            if (math.dot(flatForward, flatDir) < minDot)
            {
                los.ValueRW.Value = false;
                continue;
            }

            bool blocked = Physics.Raycast(
                origin,
                dir,
                dist,
                ai.ValueRO.ObstacleMask,
                QueryTriggerInteraction.Ignore
            );

            los.ValueRW.Value = !blocked;
        }
    }
}
