using Unity.Entities;
using Unity.Mathematics;

// 1. Тег, чтобы отличать врага от других сущностей
public struct EnemyTag : IComponentData { }

// 2. Данные самого врага (скорость движения)
public struct EnemyProperties : IComponentData
{
    public float MoveSpeed;
}

// 3. Данные для Спаунера (ссылка на префаб и настройки)
public struct EnemySpawner : IComponentData
{
    public Entity PrefabToSpawn; // Ссылка на "запеченный" префаб
    public int NumberToSpawn;    // Сколько врагов создать
    public float SpawnRadius;    // Радиус спавна
    public bool Initialized;     // Флаг, чтобы не спавнить бесконечно
}