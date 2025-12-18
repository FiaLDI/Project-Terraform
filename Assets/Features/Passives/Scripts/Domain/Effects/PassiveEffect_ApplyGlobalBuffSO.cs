using UnityEngine;
using Features.Buffs.Domain;
using Features.Buffs.UnityIntegration;

namespace Features.Passives.Domain
{
    [CreateAssetMenu(menuName = "Game/Passive/Effect/Apply Global Buff")]
    public class PassiveEffect_ApplyGlobalBuffSO : PassiveEffectSO
    {
        public GlobalBuffSO globalBuff;

        public override void Apply(GameObject owner)
        {
            if (globalBuff != null && GlobalBuffSystem.I != null)
                GlobalBuffSystem.I.Add(globalBuff);
        }

        public override void Remove(GameObject owner)
        {
            if (globalBuff != null && GlobalBuffSystem.I != null)
                GlobalBuffSystem.I.Remove(globalBuff);
        }
    }
}
