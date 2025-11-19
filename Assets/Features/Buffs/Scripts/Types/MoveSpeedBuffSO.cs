using UnityEngine;

[CreateAssetMenu(menuName = "Game/Buff/Turret/Rotation Speed")]
public class MoveSpeedBuffSO : BuffSO
{
    [Header("Multiplier")]
    public float speedMultiplier = 1.5f; // 50% faster rotation

    public override void OnApply(BuffInstance instance)
    {
        if (instance.Target.GameObject.TryGetComponent<TurretBehaviour>(out var turret))
        {
            turret.rotationSpeedMultiplier *= speedMultiplier;
        }
    }

    public override void OnExpire(BuffInstance instance)
    {
        if (instance.Target.GameObject.TryGetComponent<TurretBehaviour>(out var turret))
        {
            turret.rotationSpeedMultiplier /= speedMultiplier;
        }
    }
}
