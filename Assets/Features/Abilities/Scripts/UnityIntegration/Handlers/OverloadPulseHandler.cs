using UnityEngine;
using Features.Abilities.Domain;
using Features.Buffs.Application;
using Features.Combat.Devices;
using Features.Combat.Domain;
using Features.Buffs.Domain;

namespace Features.Abilities.UnityIntegration
{
    public class OverloadPulseHandler : IAbilityHandler
    {
        public System.Type AbilityType => typeof(OverloadPulseAbilitySO);

        public void Execute(AbilitySO abilityBase, AbilityContext ctx)
        {
            var ability = (OverloadPulseAbilitySO)abilityBase;
            var owner = ctx.Owner;
            if (!owner) return;

            Vector3 origin = owner.transform.position;

            // FX
            if (ability.pulseFxPrefab != null)
            {
                GameObject fx = Object.Instantiate(
                    ability.pulseFxPrefab,
                    origin,
                    Quaternion.identity
                );

                if (fx.TryGetComponent<OverloadPulseBehaviour>(out var pulse))
                {
                    pulse.Init(owner.transform, ability.radius, ability.fxDuration);
                }

                Object.Destroy(fx, ability.fxDuration + 1f);
            }

            // Логика баффов и урона
            Collider[] hits = Physics.OverlapSphere(origin, ability.radius);

            foreach (var h in hits)
            {
                if (h.TryGetComponent<IBuffTarget>(out var target))
                {
                    var sys = target.BuffSystem;
                    if (sys != null)
                    {
                        if (ability.damageBuff) sys.Add(ability.damageBuff);
                        if (ability.fireRateBuff) sys.Add(ability.fireRateBuff);
                        if (ability.turretMoveBuff) sys.Add(ability.turretMoveBuff);
                    }
                }

                // Урон врагам
                if (h.CompareTag("Enemy"))
                {
                    if (h.TryGetComponent<IDamageable>(out var dmg))
                        dmg.TakeDamage(ability.pulseDamage, DamageType.Generic);

                    var rb = h.attachedRigidbody;
                    if (rb != null && !rb.isKinematic)
                    {
                        Vector3 dir = h.transform.position - origin;
                        dir.y = 0f;
                        rb.AddForce(dir.normalized * ability.knockbackForce, ForceMode.Impulse);
                    }
                }
            }
        }
    }
}
