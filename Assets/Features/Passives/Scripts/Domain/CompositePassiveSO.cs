using UnityEngine;

namespace Features.Passives.Domain
{
    [CreateAssetMenu(menuName = "Game/Passive/Composite Passive")]
    public class CompositePassiveSO : PassiveSO
    {
        public PassiveEffectSO[] effects;

        public override void Apply(GameObject owner)
        {
            if (effects == null) return;
            foreach (var e in effects)
                e?.Apply(owner);
        }

        public override void Remove(GameObject owner)
        {
            if (effects == null) return;
            foreach (var e in effects)
                e?.Remove(owner);
        }
    }
}
