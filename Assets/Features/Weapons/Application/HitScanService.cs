using UnityEngine;
using Features.Weapons.Domain;
using Features.Combat.Domain;

namespace Features.Weapons.Application
{
    public class HitScanService
    {
        public WeaponHitInfo FireHitscan(
            Vector3 origin,
            Vector3 direction,
            float maxDistance,
            float damage,
            DamageType type)
        {
            WeaponHitInfo info = new WeaponHitInfo
            {
                origin = origin,
                direction = direction,
                baseDamage = damage,
                damageType = type,
                hit = false
            };

            if (Physics.Raycast(origin, direction, out var hit, maxDistance))
            {
                info.hit = true;
                info.hitPoint = hit.point;
                info.hitNormal = hit.normal;
                info.distance = hit.distance;

                if (hit.collider.GetComponentInParent<IDamageable>() is { } dmg)
                    info.target = dmg;
            }

            return info;
        }
    }
}
