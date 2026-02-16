using Features.Buffs.Application;
using Features.Buffs.UnityIntegration;
using Features.Stats.Domain;
using UnityEngine;

namespace Features.Buffs.Domain
{
    [CreateAssetMenu(menuName = "Game/Buff/Effect/Aura Emitter")]
    public sealed class BuffEffect_AuraEmitterSO : BuffEffectSO
    {
        public AreaBuffSO area;

        private AreaBuffEmitter emitter;

        public override void Apply(IStatsFacade stats)
        {
            // не используется
        }

        public override void ApplyWithContext(BuffInstance inst, IStatsFacade stats)
        {
            if (area == null || inst?.Target == null)
                return;

            emitter = inst.Target.GameObject.AddComponent<AreaBuffEmitter>();
            emitter.area = area;
        }

        public override void Tick(IStatsFacade stats, float dt) { }

        public override void Expire(IStatsFacade stats)
        {
            if (emitter != null)
                Object.Destroy(emitter);
        }
    }

}
