using UnityEngine;

[CreateAssetMenu(menuName = "Game/Ability/Overload Pulse")]
public class OverloadPulseAbilitySO : AbilitySO
{
    [Header("Buffs applied to turrets")]
    public DamageBoostBuffSO damageBuff;
    public FireRateBuffSO fireRateBuff;
    public MoveSpeedBuffSO turretMoveBuff;

    [Header("Pulse Params")]
    public float radius = 12f;
    public float pulseDamage = 35f;
    public float knockbackForce = 10f;

    [Header("FX")]
    public GameObject pulseFxPrefab;

    public override void Execute(AbilityContext context)
    {
        var owner = context.Owner;
        if (!owner) return;

        float fxDuration = damageBuff != null ? damageBuff.duration : 0.5f;

        // ---------- SPAWN FX ----------
        if (pulseFxPrefab)
        {
            Vector3 spawnPos = owner.transform.position;
            // Если владелец - голова/камера, можно принудительно опустить:
            // spawnPos.y = 0.5f;

            Debug.Log($"SPAWN FX at {spawnPos}");

            GameObject fx = Object.Instantiate(
                pulseFxPrefab,
                spawnPos,
                Quaternion.identity
            );

            Debug.Log("FX INSTANCE = " + fx, fx);

            if (fx.TryGetComponent<OverloadPulseBehaviour>(out var pulse))
            {
                Debug.Log("[Ability] Found OverloadPulseBehaviour on root, calling Init", fx);
                pulse.Init(owner.transform, radius, fxDuration);
            }
            else
            {
                Debug.LogWarning("[Ability] OverloadPulseBehaviour not found on FX root!", fx);
            }

            Object.Destroy(fx, fxDuration + 1f);
        }

        // ---------- Дальше твоя логика баффов и урона ----------
        Collider[] hits = Physics.OverlapSphere(owner.transform.position, radius);

        foreach (var h in hits)
        {
            if (h.TryGetComponent<TurretBehaviour>(out var turret))
            {
                var buffs = turret.GetComponent<BuffSystem>();
                if (buffs)
                {
                    if (damageBuff) buffs.AddBuff(damageBuff);
                    if (fireRateBuff) buffs.AddBuff(fireRateBuff);
                    if (turretMoveBuff) buffs.AddBuff(turretMoveBuff);
                }
                continue;
            }

            if (h.CompareTag("Enemy"))
            {
                if (h.TryGetComponent<IDamageable>(out var dmg))
                    dmg.TakeDamage(pulseDamage, DamageType.Generic);

                var rb = h.attachedRigidbody;
                if (rb != null && !rb.isKinematic)
                {
                    Vector3 dir = h.transform.position - owner.transform.position;
                    dir.y = 0f;
                    rb.AddForce(dir.normalized * knockbackForce, ForceMode.Impulse);
                }
            }
        }
    }
}