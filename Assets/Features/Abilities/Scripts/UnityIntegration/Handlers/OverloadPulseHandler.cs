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

            // ==== ADAPTATION: ctx.Owner is NOW object, not GameObject ====
            GameObject ownerGO = null;

            switch (ctx.Owner)
            {
                case GameObject go:
                    ownerGO = go;
                    break;

                case Component comp:
                    ownerGO = comp.gameObject;
                    break;

                default:
                    Debug.LogError("[OverloadPulse] AbilityContext.Owner is not a GameObject or Component");
                    return;
            }

            if (ownerGO == null)
                return;

            // =========================================================
            // ENSURE Buff Infrastructure
            // =========================================================

            if (PlayerDeviceBuffService.I == null)
            {
                new GameObject("PlayerDeviceBuffService")
                    .AddComponent<PlayerDeviceBuffService>();
            }

            var playerTarget = ownerGO.GetComponent<IBuffTarget>();
            if (playerTarget == null)
            {
                Debug.LogWarning("[OverloadPulse] Player has NO IBuffTarget → Adding PlayerBuffTarget");
                playerTarget = ownerGO.AddComponent<PlayerBuffTarget>();
            }

            if (playerTarget.BuffSystem == null)
            {
                Debug.LogWarning("[OverloadPulse] Player had no BuffSystem → Adding");
                ownerGO.AddComponent<BuffSystem>();
            }

            // =========================================================
            // SPAWN FX
            // =========================================================

            Vector3 origin = ownerGO.transform.position;

            if (ability.pulseFxPrefab != null)
            {
                GameObject fx = Object.Instantiate(
                    ability.pulseFxPrefab,
                    origin,
                    Quaternion.identity
                );

                if (fx.TryGetComponent<OverloadPulseBehaviour>(out var pulse))
                    pulse.Init(ownerGO.transform, ability.radius, ability.fxDuration);

                Object.Destroy(fx, ability.fxDuration + 1f);
            }

            // =========================================================
            // APPLY BUFFS TO PLAYER
            // =========================================================

            var buffSys = ownerGO.GetComponent<BuffSystem>();

            if (buffSys != null)
            {
                if (ability.damageBuff)
                    buffSys.Add(ability.damageBuff);

                if (ability.fireRateBuff)
                    buffSys.Add(ability.fireRateBuff);

                if (ability.turretMoveBuff)
                    buffSys.Add(ability.turretMoveBuff);
            }

            // =========================================================
            // APPLY BUFFS TO PLAYER DEVICES / TURRETS
            // =========================================================

            PlayerDeviceBuffService.I.BuffAllPlayerDevices(ownerGO, ability.damageBuff);
            PlayerDeviceBuffService.I.BuffAllPlayerDevices(ownerGO, ability.fireRateBuff);
            PlayerDeviceBuffService.I.BuffAllPlayerDevices(ownerGO, ability.turretMoveBuff);

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
