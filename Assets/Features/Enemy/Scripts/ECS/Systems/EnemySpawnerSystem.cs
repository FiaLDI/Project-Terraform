using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

[BurstCompile] // Магия скорости Unity
public partial struct EnemySpawnerSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        // Требуем, чтобы система работала только если есть компонент EnemySpawner
        state.RequireForUpdate<EnemySpawner>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // Получаем доступ к спаунеру. RefRW означает Read-Write (чтение и запись)
        // SystemAPI.GetSingletonRW находит единственную сущность с этим компонентом
        var spawner = SystemAPI.GetSingletonRW<EnemySpawner>();

        if (spawner.ValueRO.Initialized)
            return; // Уже заспавнили

        var entityPrefab = spawner.ValueRO.PrefabToSpawn;
        var count = spawner.ValueRO.NumberToSpawn;
        var radius = spawner.ValueRO.SpawnRadius;
        var random = new Random(1234); // Простой генератор случайных чисел

        // Создаем массив сущностей за один раз (очень быстро)
        var instances = state.EntityManager.Instantiate(entityPrefab, count, state.WorldUpdateAllocator);

        // Расставляем их
        foreach (var entity in instances)
        {
            // Случайная позиция
            float3 pos = new float3(
                random.NextFloat(-radius, radius),
                0,
                random.NextFloat(-radius, radius)
            );

            // Задаем позицию через компонент LocalTransform
            var transform = LocalTransform.FromPosition(pos);
            transform.Scale = 1f; // определение размера
            state.EntityManager.SetComponentData(entity, transform);
        }

        // Помечаем, что спавн прошел
        spawner.ValueRW.Initialized = true;
    }
}