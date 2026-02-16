
using Features.Buffs.Domain;
using Features.Passives.Domain;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Passive/Effect/Apply Aura Buff")]
public sealed class PassiveEffect_ApplyAuraBuffSO : PassiveEffectSO
{
    public BuffSO auraBuff;

    public override PassiveEffectData Build()
    {
        return new PassiveEffectData
        {
            buff = auraBuff,
            lifetime = BuffLifetimeMode.WhileSourceAlive
        };
    }
}
