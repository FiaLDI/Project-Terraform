using UnityEngine;
using Features.Abilities.Domain;
using Features.Buffs.Application;
using Features.Combat.Domain;
using Features.Buffs.Domain;
using Features.Buffs.UnityIntegration;

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

            // =========================================================
            // ENSURE Buff System Infrastructure Exists
            // =========================================================

            // 1. PlayerDeviceBuffService (для бафа турелей)
            if (PlayerDeviceBuffService.I == null)
            {
                new GameObject("PlayerDeviceBuffService")
                    .AddComponent<PlayerDeviceBuffService>();
            }

            // 2. У игрока должен быть BuffSystem (клиентский)
            var playerTarget = owner.GetComponent<IBuffTarget>();
            if (playerTarget == null)
            {
                Debug.LogWarning("[OverloadPulse] Player has NO IBuffTarget → Adding PlayerBuffTarget");
                playerTarget = owner.AddComponent<PlayerBuffTarget>();
            }

            if (playerTarget.BuffSystem == null)
            {
                Debug.LogWarning("[OverloadPulse] Player had no BuffSystem → Adding");
                owner.gameObject.AddComponent<BuffSystem>();
            }

            // =========================================================
            // SPAWN FX
            // =========================================================

            Vector3 origin = owner.transform.position;

            if (ability.pulseFxPrefab != null)
            {
                GameObject fx = Object.Instantiate(
                    ability.pulseFxPrefab,
                    origin,
                    Quaternion.identity
                );

                if (fx.TryGetComponent<OverloadPulseBehaviour>(out var pulse))
                    pulse.Init(owner.transform, ability.radius, ability.fxDuration);

                Object.Destroy(fx, ability.fxDuration + 1f);
            }


            // =========================================================
            // APPLY BUFFS TO PLAYER
            // =========================================================

            var playerSystem = owner.GetComponent<BuffSystem>();

            if (playerSystem != null)
            {
                if (ability.damageBuff)
                    playerSystem.Add(ability.damageBuff);

                if (ability.fireRateBuff)
                    playerSystem.Add(ability.fireRateBuff);

                if (ability.turretMoveBuff)
                    playerSystem.Add(ability.turretMoveBuff);
            }


            // =========================================================
            // APPLY BUFFS TO PLAYER'S TURRETS (via registry)
            // =========================================================

            PlayerDeviceBuffService.I.BuffAllPlayerDevices(owner.gameObject, ability.damageBuff);
            PlayerDeviceBuffService.I.BuffAllPlayerDevices(owner.gameObject, ability.fireRateBuff);
            PlayerDeviceBuffService.I.BuffAllPlayerDevices(owner.gameObject, ability.turretMoveBuff);


            // =========================================================
            // DAMAGE ENEMIES
            // =========================================================

            Collider[] hits = Physics.OverlapSphere(origin, ability.radius);

            foreach (var h in hits)
            {
                if (h.CompareTag("Enemy"))
                {
                    if (h.TryGetComponent<IDamageable>(out var dmg))
                        dmg.TakeDamage(ability.pulseDamage, DamageType.Generic);

                    var rb = h.attachedRigidbody;
                    if (rb)
                    {
                        Vector3 dir = h.transform.position - origin;
                        dir.y = 0;
                        rb.AddForce(dir.normalized * ability.knockbackForce, ForceMode.Impulse);
                    }
                }
            }
        }
    }
}
