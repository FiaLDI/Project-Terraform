using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

[BurstCompile]
public partial struct EnemyMovementSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;

        // Query (Запрос): дай мне всех, у кого есть LocalTransform и EnemyProperties
        // RefRW<LocalTransform> - хотим менять позицию
        // RefRO<EnemyProperties> - только читаем скорость (Read Only)
        // WithAll<EnemyTag> - фильтр, чтобы двигать только врагов

        foreach (var (transform, properties) in
                 SystemAPI.Query<RefRW<LocalTransform>, RefRO<EnemyProperties>>()
                 .WithAll<EnemyTag>())
        {
            // Простая логика: двигаем их вперед по оси Z
            float moveSpeed = properties.ValueRO.MoveSpeed;

            // transform.ValueRW.Forward() дает вектор направления "вперед"
            // Но для простоты сдвинем просто по оси Z мировых координат,
            // или используем transform.ValueRW.Translate

            float3 moveDir = new float3(0, 0, 1);

            // Обновляем позицию
            transform.ValueRW.Position += moveDir * moveSpeed * deltaTime;
        }
    }
}