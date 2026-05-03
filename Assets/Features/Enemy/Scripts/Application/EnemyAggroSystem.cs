using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(EnemyTargetingSystem))]
public partial struct EnemyAggroSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (damageBuffer, entity) in SystemAPI
            .Query<DynamicBuffer<DamageEvent>>()
            .WithEntityAccess())
        {
            if (!SystemAPI.HasBuffer<EnemyAggroElement>(entity))
                continue;

            var aggroBuffer = SystemAPI.GetBuffer<EnemyAggroElement>(entity);

            for (int i = 0; i < damageBuffer.Length; i++)
            {
                var dmg = damageBuffer[i];
                bool found = false;

                for (int j = 0; j < aggroBuffer.Length; j++)
                {
                    var entry = aggroBuffer[j];

                    if (entry.Target == dmg.Source)
                    {
                        entry.Value += dmg.Value;
                        aggroBuffer[j] = entry;
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    aggroBuffer.Add(new EnemyAggroElement
                    {
                        Target = dmg.Source,
                        Value = dmg.Value
                    });
                }
            }

            damageBuffer.Clear();
        }

        float deltaTime = SystemAPI.Time.DeltaTime;

        foreach (var (aggroBuffer, settings) in SystemAPI
                     .Query<DynamicBuffer<EnemyAggroElement>, RefRO<EnemyAggroSettings>>()
                     .WithNone<EnemyInactive>())
        {
            var buffer = aggroBuffer;
            float decay = math.max(0f, settings.ValueRO.ThreatDecayPerSecond) * deltaTime;

            for (int i = buffer.Length - 1; i >= 0; i--)
            {
                var entry = buffer[i];

                if (entry.Target == Entity.Null ||
                    !SystemAPI.Exists(entry.Target) ||
                    SystemAPI.HasComponent<PlayerDead>(entry.Target))
                {
                    buffer.RemoveAt(i);
                    continue;
                }

                entry.Value -= decay;
                if (entry.Value <= 0.01f)
                {
                    buffer.RemoveAt(i);
                    continue;
                }

                buffer[i] = entry;
            }
        }
    }
}
