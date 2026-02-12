using UnityEngine;
using Features.Combat.Domain;
using Features.Effects.Domain;

namespace Features.Weapons.Domain
{
    public struct WeaponHitInfo
    {
        public bool hit;
        public Vector3 origin;
        public Vector3 direction;
        public Vector3 hitPoint;
        public Vector3 hitNormal;
        public float distance;
        public IDamageable target;

        public DamageType damageType;
        public float baseDamage;
    }
}
