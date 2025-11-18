using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Game/Ability/OverloadPulse")]
public class OverloadPulseAbilitySO : AbilitySO
{
    public float radius = 12f;
    public float damageBonusPercent = 30f;
    public float fireRateBonusPercent = 30f;
    public float duration = 15f;

    public override void Execute(AbilityContext context)
    {
        // визуальный эффект
        if (payloadPrefab != null)
        {
            GameObject fx = Instantiate(
                payloadPrefab,
                context.Owner.transform.position,
                Quaternion.identity
            );

            var pulse = fx.GetComponent<OverloadPulseBehaviour>();
            if (pulse != null)
                pulse.Init(context.Owner.transform, radius, duration);

            Destroy(fx, 1.2f);
        }

        // баффы турелям
        Collider[] hits = Physics.OverlapSphere(context.Owner.transform.position, radius);
        foreach (var h in hits)
        {
            var turret = h.GetComponent<TurretBehaviour>();
            if (turret == null) continue;

            var buffs = turret.GetComponent<BuffSystem>();
            if (buffs != null)
            {
                buffs.AddBuff(BuffType.DamageBoost, damageBonusPercent, duration, buffIcon);
                buffs.AddBuff(BuffType.FireRateBoost, fireRateBonusPercent, duration, buffIcon);
            }
        }
    }
}
