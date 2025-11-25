using UnityEngine;

public static class DamageSystem
{
    public static float ApplyDamageModifiers(IDamageable target, float damage, DamageType type)
    {
        var targetGO = (target as MonoBehaviour)?.gameObject;
        if (targetGO == null) return damage;

        // ShieldGrid ищет активные щиты рядом с целью
        Collider[] hits = Physics.OverlapSphere(
            targetGO.transform.position,
            10f, // радиус поиска щита
            LayerMask.GetMask("Default")
        );

        foreach (var h in hits)
        {
            var grid = h.GetComponent<ShieldGridBehaviour>();
            if (grid != null && grid.IsInside(targetGO.transform.position))
            {
                damage = grid.ModifyDamage(damage);
            }
        }

        return damage;
    }
}
