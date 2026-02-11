
using Features.Buffs.Domain;
using UnityEngine;

namespace Features.Passives.Domain
{
    [CreateAssetMenu(menuName = "Game/Passive/Effect/Apply Buff")]
    public sealed class PassiveEffect_ApplyBuffSO : PassiveEffectSO
    {
        public BuffSO buff;

        public override PassiveEffectData Build()
        {
            return new PassiveEffectData
            {
                buff = buff,
                lifetime = BuffLifetimeMode.WhileSourceAlive
            };
        }
    }
}
