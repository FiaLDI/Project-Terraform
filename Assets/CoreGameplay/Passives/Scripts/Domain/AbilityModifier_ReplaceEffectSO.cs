using UnityEngine;
using System.Collections.Generic;
using Features.Effects.Domain;

[CreateAssetMenu(menuName = "Game/AbilityModifier/Replace Effect")]
public class AbilityModifier_ReplaceEffectSO : AbilityModifierSO
{
    [SerializeField] private EffectType targetType;
    [SerializeField] private EffectDefinition replacement;

    public override void Apply(List<EffectDefinition> effects)
    {
        for (int i = 0; i < effects.Count; i++)
        {
            if (effects[i].type == targetType)
            {
                effects[i] = replacement.Build();
            }
        }
    }
}