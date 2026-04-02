using UnityEngine;
using System.Collections.Generic;
using Features.Effects.Domain;

[CreateAssetMenu(menuName = "Game/AbilityModifier/Add Effect")]
public class AbilityModifier_AddEffectSO : AbilityModifierSO
{
    [SerializeField] private EffectDefinition effect;

    public override void Apply(List<EffectDefinition> effects)
    {
        effects.Add(effect.Build());
    }
}