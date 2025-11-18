using UnityEngine;

public abstract class AbilitySO : ScriptableObject
{
    [Header("UI")]
    public string id;
    public string displayName;
    public Sprite icon;

    [Header("Cast")]
    public float energyCost = 20f;
    public float cooldown = 12f;
    public AbilityTarget targetType = AbilityTarget.Self;
    public AbilityCastType castType = AbilityCastType.Instant;
    public float castTime = 0f;

    [Header("Payload")]
    public GameObject payloadPrefab;

    /// <summary>
    /// Собственно логика способности. Вызывается в пик анимации (OnCastImpact).
    /// </summary>
    public abstract void Execute(AbilityContext context);
}
