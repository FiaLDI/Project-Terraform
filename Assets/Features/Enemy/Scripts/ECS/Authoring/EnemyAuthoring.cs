using Unity.Entities;
using UnityEngine;

public class EnemyAuthoring : MonoBehaviour
{
    public float MoveSpeed = 5f;

    // Класс-бейкер преобразует MonoBehaviour данные в ECS компоненты
    class Baker : Baker<EnemyAuthoring>
    {
        public override void Bake(EnemyAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            // Добавляем наши компоненты
            AddComponent(entity, new EnemyTag());
            AddComponent(entity, new EnemyProperties
            {
                MoveSpeed = authoring.MoveSpeed
            });
        }
    }
}