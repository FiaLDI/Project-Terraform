using UnityEngine;

namespace Features.Passives.Domain
{
    [CreateAssetMenu(menuName = "Game/Passive/Composite Passive")]
    public sealed class CompositePassiveSO : PassiveSO
    {
        public PassiveEffectSO[] effects;

        protected override void ApplyInternal(GameObject owner)
        {
            if (effects == null)
                return;

            foreach (var e in effects)
                e?.Apply(owner);
        }

        protected override void RemoveInternal(GameObject owner)
        {
            if (effects == null)
                return;

            foreach (var e in effects)
                e?.Remove(owner);
        }
    }
}
