using UnityEngine;
using Features.Effects.Domain;

public enum ItemActionType
{
    Primary,
    Secondary,
    Reload,
    Alt
}

public enum ItemActionState
{
    Idle,
    Windup,
    Active,
    Cooldown,
    Reloading
}

[System.Serializable]
public class ItemActionDefinition
{
    public ItemActionType actionType;

    [Header("Timing")]
    public float cooldown;
    public float tickInterval = 0.1f;

    [Header("Optional States")]
    public float windupTime;
    public bool fireOnRelease;
    public float reloadTime;

    [Header("Burst")]
    public int burstCount;
    public float burstInterval;

    [Header("Effects")]
    public EffectDefinition[] effects;
}
