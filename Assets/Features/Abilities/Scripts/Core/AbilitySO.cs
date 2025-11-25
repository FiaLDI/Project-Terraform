using UnityEngine;

public abstract class AbilitySO : ScriptableObject
{
    [Header("UI")]
    public string id;
    public string displayName;
    public Sprite icon;
    public Sprite buffIcon;

    [Header("Cast Settings")]
    public float energyCost = 20f;
    public float cooldown = 12f;
    public AbilityTarget targetType = AbilityTarget.Self;
    public AbilityCastType castType = AbilityCastType.Instant;
    public float castTime = 0f;

    public abstract void Execute(AbilityContext context);
}
