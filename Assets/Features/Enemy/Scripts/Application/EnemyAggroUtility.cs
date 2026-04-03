using Unity.Entities;

public static class EnemyAggroUtility
{
    public static void AddAggro(EntityManager em, Entity enemy, Entity source, float value)
    {
        if (!em.Exists(enemy)) return;
        if (!em.HasBuffer<EnemyAggroElement>(enemy)) return;

        var buffer = em.GetBuffer<EnemyAggroElement>(enemy);

        for (int i = 0; i < buffer.Length; i++)
        {
            if (buffer[i].Target == source)
            {
                var a = buffer[i];
                a.Value += value;
                buffer[i] = a;
                return;
            }
        }

        buffer.Add(new EnemyAggroElement
        {
            Target = source,
            Value = value
        });
    }
}
