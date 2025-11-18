using UnityEngine;

[CreateAssetMenu(menuName = "Game/Ability/OverloadPulse")]
public class OverloadPulseAbilitySO : AbilitySO
{
    public float radius = 12f;
    public float damageBonusPercent = 30f;
    public float fireRateBonusPercent = 30f;
    public float duration = 15f;

    public override void Execute(AbilityContext context)
    {
        if (payloadPrefab != null)
        {
            GameObject.Instantiate(
                payloadPrefab,
                context.Owner.transform.position,
                Quaternion.identity
            );
        }

        Collider[] hits = Physics.OverlapSphere(context.Owner.transform.position, radius);
        foreach (var h in hits)
        {
            var turret = h.GetComponent<TurretBehaviour>();
            if (turret == null) continue;

            turret.StartCoroutine(ApplyBuffCoroutine(turret));
        }
    }

    private System.Collections.IEnumerator ApplyBuffCoroutine(TurretBehaviour turret)
    {
        float originalDps = turret.damagePerSecond;

        turret.damagePerSecond = originalDps * (1f + damageBonusPercent / 100f);
        // Если есть отдельный параметр fireRate — аналогично бустим его

        yield return new WaitForSeconds(duration);

        if (turret != null)
        {
            turret.damagePerSecond = originalDps;
        }
    }
}
