using Features.Buffs.Domain;
using UnityEngine;

namespace Features.Passives.Domain
{
    public abstract class PassiveEffectSO : ScriptableObject, IBuffSource
    {
        public abstract void Apply(GameObject owner);
        public abstract void Remove(GameObject owner);
    }
}
