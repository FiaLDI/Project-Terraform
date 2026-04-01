using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

// =========================
// BASE AI
// =========================
public struct EnemyAI : IComponentData
{
    public float MoveSpeed;
    public float AggroRadius;
    public float LoseAggroRadius;

    public float AttackRange;
    public float AttackCooldown;

    public float AttackEnterOffset;
    public float AttackExitOffset;
    public float StopDistanceMultiplier;

    public float VisionAngle;
    public float VisionRange;
    public bool RequireLOS;

    public int ObstacleMask;
}

public struct EnemyTarget : IComponentData
{
    public Entity Value;
}

public struct EnemyAggroSettings : IComponentData
{
    public float SwitchDistance;
    public float LoseDistance; 
}

public struct EnemyAggroElement : IBufferElementData
{
    public Entity Target;
    public float Value;
}

public struct DamageEvent : IBufferElementData
{
    public Entity Source;
    public float Value;
}

public struct EnemyHasLineOfSight : IComponentData
{
    public bool Value;
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

// =========================
// TARGETING
// =========================
public struct EnemyTargetPosition : IComponentData
{
    public float3 Value;
}

public struct EnemyLastKnownPosition : IComponentData
{
    public float3 Value;
}

// =========================
// AGGRO
// =========================
public struct EnemyAggroState : IComponentData
{
    public float Timer;
}

// =========================
// ATTACK
// =========================
public struct EnemyAttackState : IComponentData
{
    public bool DoAttack;
    public bool IsAttacking;

    public float Cooldown;
    public float Timer;

    public EnemyAttackType Type;
}

// =========================
// PATROL
// =========================
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

public struct EnemyInactive : IComponentData {}

public struct EnemyPatrolSettings : IComponentData
{
    public float ReachDistance;

    public float MinWaitTime;
    public float MaxWaitTime;

    public bool RandomPatrol;
}

public struct EnemyAttackSettings : IComponentData
{
    public float MeleeRange;
    public float RangedRange;

    public float MeleeCooldown;
    public float RangedCooldown;
}


public class EnemyChunkLink : MonoBehaviour
{
    public Vector2Int chunkCoord;
}

public enum EnemyAttackType : byte
{
    None,
    Melee,
    Ranged
}

// =========================
// OTHER
// =========================
public struct EnemyBlocked : IComponentData
{
    public bool Value;
}

public struct EnemyTag : IComponentData { }
public struct PlayerTag : IComponentData { }
