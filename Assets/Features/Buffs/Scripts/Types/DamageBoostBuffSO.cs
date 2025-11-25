using UnityEngine;

[CreateAssetMenu(menuName = "Game/Buff/Turret/Damage Boost")]
public class DamageBoostBuffSO : BuffSO
{
    public float multiplier = 1.3f;

    public override void OnApply(BuffInstance instance)
    {
        if (instance.Target.GameObject.TryGetComponent<TurretBehaviour>(out var turret))
        {
            turret.damageMultiplier *= multiplier;
        }
    }

    public override void OnExpire(BuffInstance instance)
    {
        if (instance.Target.GameObject.TryGetComponent<TurretBehaviour>(out var turret))
        {
            turret.damageMultiplier /= multiplier;
        }
    }
}
