using Features.Buffs.Domain;
using Features.Buffs.UnityIntegration;
using UnityEngine;

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

            var runtime = new PassiveRuntime();

            GlobalBuffSystem.I.Add(
                globalBuff,
                source: runtime
            );

            PassiveRuntimeRegistry.Store(owner, this, runtime);
        }

        public override void Remove(GameObject owner)
        {
            var runtime = PassiveRuntimeRegistry.Take(owner, this);
            if (runtime == null)
                return;

            GlobalBuffSystem.I.RemoveBySource(runtime);
        }
    }
}
