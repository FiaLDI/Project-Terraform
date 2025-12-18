using Features.Combat.Devices;
using Features.Combat.Domain;
using UnityEngine;

namespace Features.Combat.Application
{
    public class CombatService
    {
        private readonly DamageCalculationService calc = new();
        private readonly DoTEffectService dots = new();

        public float ApplyEnvironmentalModifiers(IDamageable target, float dmg, DamageType type)
        {
            // интеграция старого DamageSystem
            var targetGO = (target as MonoBehaviour)?.gameObject;
            if (targetGO == null) return dmg;

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
                    dmg = grid.ModifyDamage(dmg);
                }
            }

            return dmg;
        }

        public void ApplyDamage(IDamageable target, HitInfo hit, DamageModifiers mods)
        {
            // 1) базовая формула (резисты + множители)
            float dmg = calc.CalculateFinalDamage(hit, target.GetResistProfile(), mods);

            // 2) ShieldGrid / Damage devices (legacy support)
            dmg = ApplyEnvironmentalModifiers(target, dmg, hit.type);

            // 3) Отправка урона объекту
            target.ApplyDamage(dmg, hit.type, hit);
        }

        public void ApplyDot(IDamageable target, DoTEffectData dot)
        {
            dots.ApplyDot(target, dot);
        }

        public void Tick(float dt)
        {
            dots.Tick(dt);
        }
    }
}
