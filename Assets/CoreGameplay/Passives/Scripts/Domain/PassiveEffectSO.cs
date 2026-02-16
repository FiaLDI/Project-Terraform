
using UnityEngine;

namespace Features.Passives.Domain
{
    public abstract class PassiveEffectSO : ScriptableObject
    {
        public abstract PassiveEffectData Build();
    }

}
