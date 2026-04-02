using UnityEngine;
using System.Collections.Generic;
using Features.Effects.Domain;

public abstract class AbilityModifierSO : ScriptableObject
{
    [SerializeField] protected AbilityModifierTarget target;

    public bool Matches(string abilityId, AbilityTag tags)
        => target.Matches(abilityId, tags);

    public abstract void Apply(List<EffectDefinition> effects);
}
