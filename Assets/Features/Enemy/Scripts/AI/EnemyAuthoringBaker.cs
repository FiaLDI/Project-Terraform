using Unity.Entities;
using Unity.Mathematics;

public class EnemyAuthoringBaker : Baker<EnemyAuthoring>
{
    public override void Bake(EnemyAuthoring authoring)
    {
        Entity entity = GetEntity(TransformUsageFlags.Dynamic);

        // TAG
        AddComponent<EnemyTag>(entity);

        // AI DATA
        AddComponent(entity, new EnemyAI
        {
            MoveSpeed = authoring.MoveSpeed,
            AggroRadius = authoring.AggroRadius
        });

        // STATE
        AddComponent(entity, new EnemyState
        {
            Value = EnemyAIState.Patrol
        });

        // TARGET POSITION
        AddComponent(entity, new EnemyTargetPosition
        {
            Value = authoring.transform.position
        });
    }
}
