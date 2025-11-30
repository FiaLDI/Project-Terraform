using UnityEngine;
using Features.Buffs.Domain;
using Features.Buffs.UnityIntegration;

namespace Features.Passives.Domain
{
    [CreateAssetMenu(menuName = "Game/Passive/Effect/Aura Emitter")]
    public class PassiveEffect_AuraEmitterSO : PassiveEffectSO
    {
        public AreaBuffSO aura;
        private AreaBuffEmitter _emitter;

        public override void Apply(GameObject owner)
        {
            if (aura == null) return;

            _emitter = owner.AddComponent<AreaBuffEmitter>();
            _emitter.area = aura;
        }

        public override void Remove(GameObject owner)
        {
            if (_emitter != null)
            {
                Object.Destroy(_emitter);
                _emitter = null;
            }
        }
    }
}
