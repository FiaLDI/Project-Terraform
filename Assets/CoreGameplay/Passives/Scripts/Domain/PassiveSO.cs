
using UnityEngine;

namespace Features.Passives.Domain
{
    [CreateAssetMenu(menuName = "Game/Passive/PassiveSO")]
    public sealed class PassiveSO : ScriptableObject
    {
        public string id;
        public PassiveEffectSO[] effects;
    }
}
