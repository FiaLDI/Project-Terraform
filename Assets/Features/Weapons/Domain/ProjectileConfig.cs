using UnityEngine;
using Features.Combat.Domain;

namespace Features.Weapons.Domain
{
    [CreateAssetMenu(menuName = "Weapons/Projectile Config")]
    public class ProjectileConfig : ScriptableObject
    {
        [Header("Prefab")]
        public GameObject projectilePrefab;

        [Header("Physics")]
        public float speed = 30f;
        public float lifetime = 5f;
        public bool useGravity = false;

        [Header("Damage")]
        public float damage = 10f;
        public DamageType damageType = DamageType.Generic;

        [Header("Collision")]
        public LayerMask hitMask;
        public bool destroyOnHit = true;

        [Header("FX")]
        public GameObject hitEffect;
    }
}
