using UnityEngine;
using Features.Buffs.Domain;
using Features.Buffs.UnityIntegration;

namespace Features.Passives.Domain
{
    [CreateAssetMenu(menuName = "Game/Passive/Effect/Aura Emitter")]
    public sealed class PassiveEffect_AuraEmitterSO : PassiveEffectSO
    {
        public AreaBuffSO aura;

        public override void Apply(GameObject owner)
        {
            if (aura == null)
                return;

            var runtime = new PassiveRuntime();
            var emitter = owner.AddComponent<AreaBuffEmitter>();
            emitter.area = aura;

            runtime.Component = emitter;
            PassiveRuntimeRegistry.Store(owner, this, runtime);
        }

        public override void Remove(GameObject owner)
        {
            var runtime = PassiveRuntimeRegistry.Take(owner, this);
            if (runtime?.Component != null)
                Object.Destroy(runtime.Component);
        }
    }
}
