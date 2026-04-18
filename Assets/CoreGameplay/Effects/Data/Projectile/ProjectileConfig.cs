using UnityEngine;
using Features.Effects.Domain;

namespace Features.Weapons.Domain
{
    public enum ProjectileVisualType
    {
        Projectile,
        Trail,
        Laser
    }

    [CreateAssetMenu(menuName = "Items/Configs/Projectile Config")]
    public class ProjectileConfig : ScriptableObject
    {
        [Header("Server")]
        public bool useServerProjectile;
        public GameObject projectilePrefab;

        [Header("Client (FPS local only)")]
        public GameObject clientProjectilePrefab;

        public ProjectileVisualType visualType;

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
