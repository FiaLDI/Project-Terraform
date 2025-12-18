using System.Collections.Generic;
using UnityEngine;
using Features.Weapons.Domain;
using Features.Combat.Domain;
using Features.Weapons.Data;

namespace Features.Weapons.Application
{
    public class MeleeService
    {
        public struct MeleeHit
        {
            public IDamageable target;
            public Vector3 hitPoint;
            public Vector3 hitNormal;
            public float distance;
        }

        public IEnumerable<MeleeHit> PerformMeleeAttack(
            WeaponConfig config,
            Vector3 origin,
            Vector3 forward)
        {
            List<MeleeHit> hits = new();

            Collider[] cols = Physics.OverlapSphere(origin, config.meleeRange);

            foreach (var col in cols)
            {
                if (!col.TryGetComponent<IDamageable>(out var dmg))
                    continue;

                Vector3 dirToTarget = (col.transform.position - origin).normalized;

                float angle = Vector3.Angle(forward, dirToTarget);
                if (angle > config.meleeAngle)
                    continue;

                if (Physics.Raycast(origin, dirToTarget, out var hit, config.meleeRange))
                {
                    hits.Add(new MeleeHit
                    {
                        target = dmg,
                        hitPoint = hit.point,
                        hitNormal = hit.normal,
                        distance = hit.distance
                    });
                }
            }

            return hits;
        }
    }
}
