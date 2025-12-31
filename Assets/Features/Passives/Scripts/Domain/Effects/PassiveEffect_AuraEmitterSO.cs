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

            // защита от дублей
            if (owner.GetComponent<AreaBuffEmitter>() != null)
                return;

            var emitter = owner.AddComponent<AreaBuffEmitter>();
            emitter.area = aura;
        }

        public override void Remove(GameObject owner)
        {
            var emitter = owner.GetComponent<AreaBuffEmitter>();
            if (emitter != null)
                Object.Destroy(emitter);
        }
    }
}
