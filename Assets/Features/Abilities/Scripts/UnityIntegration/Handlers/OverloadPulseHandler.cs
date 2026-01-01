using UnityEngine;
using Features.Buffs.Application;
using Features.Buffs.Domain;
using Features.Combat.Domain;
using Features.Abilities.Domain;

namespace Features.Abilities.UnityIntegration
{
    public sealed class OverloadPulseHandler
        : AbilityHandler<OverloadPulseAbilitySO>
    {
        protected override void ExecuteInternal(
            OverloadPulseAbilitySO ability,
            AbilityContext ctx,
            GameObject owner)
        {
            EnsureBuffInfrastructure(owner);

            Vector3 origin = owner.transform.position;

            // ================= FX =================
            if (ability.pulseFxPrefab != null)
            {
                var fx = Object.Instantiate(
                    ability.pulseFxPrefab,
                    origin,
                    Quaternion.identity
                );

                if (fx.TryGetComponent(out OverloadPulseBehaviour pulse))
                    pulse.Init(owner.transform, ability.radius, ability.fxDuration);

                Object.Destroy(fx, ability.fxDuration + 1f);
            }

            // ================= PLAYER BUFFS =================
            var buffSys = owner.GetComponent<BuffSystem>();
            if (buffSys != null)
            {
                Apply(buffSys, ability.damageBuff, ability);
                Apply(buffSys, ability.fireRateBuff, ability);
                Apply(buffSys, ability.turretMoveBuff, ability);
            }

            // ================= DEVICES =================
            PlayerDeviceBuffService.I.BuffAllPlayerDevices(
                owner, ability.damageBuff, ability);

            PlayerDeviceBuffService.I.BuffAllPlayerDevices(
                owner, ability.fireRateBuff, ability);

            PlayerDeviceBuffService.I.BuffAllPlayerDevices(
                owner, ability.turretMoveBuff, ability);

            // ================= DAMAGE =================
            foreach (var h in Physics.OverlapSphere(origin, ability.radius))
            {
                if (!h.CompareTag("Enemy"))
                    continue;

                if (h.TryGetComponent<IDamageable>(out var dmg))
                    dmg.TakeDamage(ability.pulseDamage, DamageType.Generic);

                if (h.attachedRigidbody != null)
                {
                    Vector3 dir = h.transform.position - origin;
                    dir.y = 0;

                    h.attachedRigidbody.AddForce(
                        dir.normalized * ability.knockbackForce,
                        ForceMode.Impulse
                    );
                }
            }
        }

        private static void Apply(BuffSystem sys, BuffSO buff, AbilitySO source)
        {
            if (buff == null) return;

            sys.Add(
                buff,
                source: source,
                lifetimeMode: BuffLifetimeMode.Duration
            );
        }

        private static void EnsureBuffInfrastructure(GameObject owner)
        {
            if (owner.GetComponent<IBuffTarget>() == null)
                owner.AddComponent<PlayerBuffTarget>();

            if (owner.GetComponent<BuffSystem>() == null)
                owner.AddComponent<BuffSystem>();

            if (PlayerDeviceBuffService.I == null)
                new GameObject("PlayerDeviceBuffService")
                    .AddComponent<PlayerDeviceBuffService>();
        }
    }
}
