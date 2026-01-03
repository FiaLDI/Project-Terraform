using Unity.Entities;
using Unity.Mathematics;

public struct EnemyAI : IComponentData
{
    public float MoveSpeed;
    public float AggroRadius;
    public float LoseAggroRadius;
}

public struct EnemyState : IComponentData
{
    public EnemyAIState Value;
}

public enum EnemyAIState : byte
{
    Patrol,
    Attack,
    Chase,
    Return,
}

public struct EnemyReturnTarget : IComponentData
{
    public float3 Position;
}

public struct EnemyTargetPosition : IComponentData
{
    public float3 Value;
}

public struct EnemySpawner : IComponentData
{
    public Entity PrefabToSpawn;
    public int NumberToSpawn;
    public float SpawnRadius;
    public bool Initialized;
}

public struct EnemyPatrolPoint : IBufferElementData
{
    public float3 Position;
}

public struct EnemyPatrolState : IComponentData
{
    public int CurrentIndex;

    public float WaitTimer;
    public float CurrentWaitDuration;

    public bool IsWaiting;
}

public struct EnemyPatrolSettings : IComponentData
{
    public float ReachDistance;

    public float MinWaitTime;
    public float MaxWaitTime;

    public bool RandomPatrol;
}

public struct EnemyBlocked : IComponentData
{
    public bool Value;
}

public struct PlayerTag : IComponentData { }
public struct EnemyTag : IComponentData { }

