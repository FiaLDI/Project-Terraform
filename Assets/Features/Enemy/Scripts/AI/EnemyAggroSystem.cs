using Unity.Burst;
using Unity.Entities;

[BurstCompile]
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
    }
}
