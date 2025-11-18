using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Game/Ability/OverloadPulse")]
public class OverloadPulseAbilitySO : AbilitySO
{
    [Header("Buff for devices")]
    public float radius = 12f;
    public float damageBonusPercent = 30f;
    public float fireRateBonusPercent = 30f;
    public float duration = 15f;

    [Header("Impact on enemies")]
    public float pulseDamage = 35f;
    public float knockbackForce = 10f;

    public override void Execute(AbilityContext context)
    {
        var owner = context.Owner;
        if (!owner) return;

        // Визуальный оверлоад-пульс
        if (payloadPrefab != null)
        {
            GameObject fx = Instantiate(
                payloadPrefab,
                owner.transform.position,
                Quaternion.identity
            );

            var pulse = fx.GetComponent<OverloadPulseBehaviour>();
            if (pulse != null)
                pulse.Init(owner.transform, radius, duration);

            Destroy(fx, duration);
        }

        Collider[] hits = Physics.OverlapSphere(owner.transform.position, radius);

        foreach (var h in hits)
        {
            // 1) Бафф турелям
            if (h.TryGetComponent<TurretBehaviour>(out var turret))
            {
                var buffs = turret.GetComponent<BuffSystem>();
                if (buffs != null)
                {
                    if (damageBonusPercent != 0)
                        buffs.AddBuff(BuffType.DamageBoost, damageBonusPercent, duration, buffIcon);

                    if (fireRateBonusPercent != 0)
                        buffs.AddBuff(BuffType.FireRateBoost, fireRateBonusPercent, duration, buffIcon);
                }

                continue; // чтобы не обрабатывать турель как врага
            }

            // 2) Враги — урон + откидывание
            if (h.CompareTag("Enemy"))
            {
                // Урон
                if (h.TryGetComponent<IDamageable>(out var dmg))
                {
                    dmg.TakeDamage(pulseDamage, DamageType.Generic);
                }

                // Откидывание (нужен Rigidbody у врага)
                var rb = h.attachedRigidbody;
                if (rb != null && !rb.isKinematic)
                {
                    Vector3 dir = (h.transform.position - owner.transform.position);
                    dir.y = 0f; // без сильного подкидывания
                    dir = dir.normalized;

                    rb.AddForce(dir * knockbackForce, ForceMode.Impulse);
                }
            }
        }
    }
}
