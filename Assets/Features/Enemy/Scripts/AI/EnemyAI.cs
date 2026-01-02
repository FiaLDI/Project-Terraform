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
    Chase
}

public struct EnemyTargetPosition : IComponentData
{
    public float3 Value;
}

public struct PlayerTag : IComponentData { }
public struct EnemyTag : IComponentData { }
