using Unity.Entities;

public struct EnemyDespawnDistance : IComponentData
{
    public float Value;
}

public struct EnemyMarkedForDespawn : IComponentData {}