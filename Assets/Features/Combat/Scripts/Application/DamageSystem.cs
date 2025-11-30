using UnityEngine;
using Features.Combat.Domain;
using Features.Combat.Devices;

namespace Features.Combat.Application
{
    public static class DamageSystem
    {
        public static float ApplyDamageModifiers(IDamageable target, float damage, DamageType type)
        {
            var targetGO = (target as MonoBehaviour)?.gameObject;
            if (targetGO == null) return damage;

            Collider[] hits = Physics.OverlapSphere(
                targetGO.transform.position,
                10f,
                LayerMask.GetMask("Default")
            );

            foreach (var h in hits)
            {
                var grid = h.GetComponent<ShieldGridBehaviour>();
                if (grid != null && grid.IsInside(targetGO.transform.position))
                {
                    damage = grid.ModifyDamage(damage);
                }
            }

            return damage;
        }
    }
}
