using UnityEngine;
using Features.Buffs.Domain;
using Features.Buffs.UnityIntegration;

namespace Features.Passives.Domain
{
    [CreateAssetMenu(menuName = "Game/Passive/Effect/Apply Global Buff")]
    public sealed class PassiveEffect_ApplyGlobalBuffSO : PassiveEffectSO
    {
        public GlobalBuffSO globalBuff;

        public override void Apply(GameObject owner)
        {
            if (globalBuff == null || GlobalBuffSystem.I == null)
                return;

            // source = этот PassiveEffect
            GlobalBuffSystem.I.Add(
                globalBuff,
                source: this
            );
        }

        public override void Remove(GameObject owner)
        {
            if (globalBuff == null || GlobalBuffSystem.I == null)
                return;

            GlobalBuffSystem.I.RemoveBySource(this);
        }
    }
}
