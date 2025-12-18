using UnityEngine;

namespace Features.Enemy.Domain
{
    [System.Serializable]
    public class HitboxProfile
    {
        public HitboxType type;
        public float damageMultiplier = 1f;
    }
}
