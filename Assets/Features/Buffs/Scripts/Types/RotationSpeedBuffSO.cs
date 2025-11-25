using UnityEngine;

[CreateAssetMenu(menuName = "Game/Buff/Turret/Rotation Speed")]
public class RotationSpeedBuffSO : BuffSO
{
    [Header("Multipliers")]
    public float multiplier = 1.5f; // +50% скорость поворота

    public override void OnApply(BuffInstance instance)
    {
        if (instance.Target.GameObject.TryGetComponent<TurretBehaviour>(out var turret))
            turret.rotationSpeedMultiplier *= multiplier;
    }

    public override void OnExpire(BuffInstance instance)
    {
        if (instance.Target.GameObject.TryGetComponent<TurretBehaviour>(out var turret))
            turret.rotationSpeedMultiplier /= multiplier;
    }
}
