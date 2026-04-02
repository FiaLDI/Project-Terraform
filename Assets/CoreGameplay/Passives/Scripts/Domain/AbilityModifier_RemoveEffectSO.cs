using UnityEngine;
using System.Collections.Generic;
using Features.Effects.Domain;

[CreateAssetMenu(menuName = "Game/AbilityModifier/Remove Effect")]
public class AbilityModifier_RemoveEffectSO : AbilityModifierSO
{
    [SerializeField] private EffectType removeType;

    public override void Apply(List<EffectDefinition> effects)
    {
        for (int i = effects.Count - 1; i >= 0; i--)
        {
            if (effects[i].type == removeType)
                effects.RemoveAt(i);
        }
    }
}
