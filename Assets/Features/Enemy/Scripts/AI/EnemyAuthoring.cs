using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class EnemyAuthoring : MonoBehaviour
{
    public float MoveSpeed = 3f;
    public float AggroRadius = 6f;
    public float LoseAggroRadius = 8f;

    class Baker : Baker<EnemyAuthoring>
    {
        public override void Bake(EnemyAuthoring authoring) 
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new EnemyTag());

            AddComponent(entity, new EnemyAI
            {
                MoveSpeed = authoring.MoveSpeed,
                AggroRadius = authoring.AggroRadius,
                LoseAggroRadius = authoring.LoseAggroRadius
            });

            AddComponent(entity, new EnemyState
            {
                Value = EnemyAIState.Patrol
            });

            AddComponent(entity, new EnemyTargetPosition
            {
                Value = float3.zero
            });
        }
    }
}
