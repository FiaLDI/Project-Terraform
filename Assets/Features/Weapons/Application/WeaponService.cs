using Features.Combat.Domain;
using Features.Effects.Domain;
using Features.Stats.Domain;
using UnityEngine;

namespace Features.Weapons.Application
{
    public class WeaponService
    {
        private ICombatStats stats;
        private float nextFireTime;

        public void Initialize(ICombatStats stats)
        {
            this.stats = stats;
            nextFireTime = 0f;
        }

        public bool CanShoot(float currentTime)
        {
            if (stats == null)
                return false;

            float fireRate = stats.FireRate;
            float delay = fireRate > 0f ? 1f / fireRate : 0f;

            return currentTime >= nextFireTime;
        }

        public void RegisterShot(float currentTime)
        {
            float fireRate = stats.FireRate;
            float delay = fireRate > 0f ? 1f / fireRate : 0f;

            nextFireTime = currentTime + delay;
        }

        public HitInfo CreateHit(
            float baseDamage,
            Vector3 hitPoint,
            Vector3 direction,
            DamageType damageType)
        {
            float finalDamage = baseDamage;

            if (stats != null)
                finalDamage *= stats.DamageMultiplier;

            return new HitInfo
            {
                damage = finalDamage,
                type = damageType,
                point = hitPoint,
                direction = direction
            };
        }

        public float GetSpread(bool aiming)
        {
            if (stats == null)
                return 0f;

            return aiming ? stats.AimSpread : stats.Spread;
        }

        public float GetRange()
        {
            return stats?.Range ?? 0f;
        }
    }
}
