using Features.Combat.Domain;
using Features.Weapons.Domain;
using UnityEngine;

namespace Features.Weapons.Application
{
    public class WeaponService
    {
        private WeaponRuntimeStats stats;
        private float nextFireTime;

        public void Initialize(WeaponRuntimeStats stats)
        {
            this.stats = stats;
            nextFireTime = 0f;
        }

        public bool CanShoot(float currentTime)
        {
            return currentTime >= nextFireTime;
        }

        public void RegisterShot(float currentTime)
        {
            // fireRate = shots per second
            float delay = stats.fireRate > 0f ? 1f / stats.fireRate : 0f;
            nextFireTime = currentTime + delay;
        }

        public HitInfo CreateHit(
            Vector3 hitPoint,
            Vector3 direction,
            DamageType damageType)
        {
            return new HitInfo
            {
                damage = stats.damage,
                type = damageType,
                point = hitPoint,
                direction = direction
            };
        }

        public float GetSpread(bool aiming)
        {
            return aiming ? stats.aimSpread : stats.spread;
        }
    }
}
