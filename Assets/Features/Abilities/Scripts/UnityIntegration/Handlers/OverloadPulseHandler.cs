using UnityEngine;
using Features.Abilities.Domain;
using Features.Buffs.Application;
using Features.Buffs.Domain;
using Features.Combat.Domain;
using Features.Buffs.UnityIntegration;
using FishNet.Object;

namespace Features.Abilities.UnityIntegration
{
    public sealed class OverloadPulseHandler : IAbilityHandler
    {
        public System.Type AbilityType => typeof(OverloadPulseAbilitySO);

        public void Execute(AbilitySO abilityBase, AbilityContext ctx)
        {
            var ability = (OverloadPulseAbilitySO)abilityBase;

            if (!TryResolveOwner(ctx.Owner, out var ownerGO))
                return;

            var netObj = ownerGO.GetComponent<NetworkObject>();
            if (netObj != null && !netObj.IsServer)
                return;

            EnsureBuffInfrastructure(ownerGO);

            // ================= FX =================

            Vector3 origin = ownerGO.transform.position;

            if (ability.pulseFxPrefab != null)
            {
                var fx = Object.Instantiate(
                    ability.pulseFxPrefab,
                    origin,
                    Quaternion.identity
                );

                if (fx.TryGetComponent<OverloadPulseBehaviour>(out var pulse))
                    pulse.Init(ownerGO.transform, ability.radius, ability.fxDuration);

                Object.Destroy(fx, ability.fxDuration + 1f);
            }

            // ================= PLAYER BUFFS =================

            var buffSys = ownerGO.GetComponent<BuffSystem>();
            if (buffSys != null)
            {
                Apply(buffSys, ability.damageBuff, ability);
                Apply(buffSys, ability.fireRateBuff, ability);
                Apply(buffSys, ability.turretMoveBuff, ability);
            }

            // ================= DEVICES =================

            PlayerDeviceBuffService.I.BuffAllPlayerDevices(
                ownerGO,
                ability.damageBuff,
                source: ability
            );

            PlayerDeviceBuffService.I.BuffAllPlayerDevices(
                ownerGO,
                ability.fireRateBuff,
                source: ability
            );

            PlayerDeviceBuffService.I.BuffAllPlayerDevices(
                ownerGO,
                ability.turretMoveBuff,
                source: ability
            );

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

        private static bool TryResolveOwner(object owner, out GameObject go)
        {
            go = owner switch
            {
                GameObject g => g,
                Component c => c.gameObject,
                _ => null
            };
            return go != null;
        }
    }
}
