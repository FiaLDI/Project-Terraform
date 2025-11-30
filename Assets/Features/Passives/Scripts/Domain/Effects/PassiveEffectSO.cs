using UnityEngine;

namespace Features.Passives.Domain
{
    public abstract class PassiveEffectSO : ScriptableObject
    {
        public abstract void Apply(GameObject owner);
        public abstract void Remove(GameObject owner);
    }
}
