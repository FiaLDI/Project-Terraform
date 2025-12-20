using UnityEngine;

namespace Features.Buffs.Domain
{
    [CreateAssetMenu(menuName = "Game/Buff/Global Buff")]
    public class GlobalBuffSO : ScriptableObject
    {
        public string key;
        public float value;
    }
}
