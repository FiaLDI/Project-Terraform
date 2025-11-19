using UnityEngine;

[CreateAssetMenu(menuName = "Game/Buff/Turret/Fire Rate")]
public class FireRateBuffSO : BuffSO
{
    public float multiplier = 1.4f;

    public override void OnApply(BuffInstance instance)
    {
        if (instance.Target.GameObject.TryGetComponent<TurretBehaviour>(out var turret))
        {
            turret.fireRateMultiplier *= multiplier;
        }
    }

    public override void OnExpire(BuffInstance instance)
    {
        if (instance.Target.GameObject.TryGetComponent<TurretBehaviour>(out var turret))
        {
            turret.fireRateMultiplier /= multiplier;
        }
    }
}
