using UnityEngine;

namespace Features.Buffs.Domain
{
    [CreateAssetMenu(menuName = "Orbis/Buffs/Global Buff")]
    public class GlobalBuffSO : ScriptableObject
    {
        public string key;
        public float value;
    }
}
