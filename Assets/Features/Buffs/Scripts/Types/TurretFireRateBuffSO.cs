using UnityEngine;

[CreateAssetMenu(menuName = "Game/Buff/Turret Fire Rate")]
public class TurretFireRateBuffSO : BuffSO
{
    public float bonusRate = 0.35f;

    public override void OnApply(BuffInstance instance)
    {
        var turret = instance.Target.Transform.GetComponent<TurretBehaviour>();
        if (turret)
            turret.fireRateMultiplier += bonusRate;
    }

    public override void OnExpire(BuffInstance instance)
    {
        var turret = instance.Target.Transform.GetComponent<TurretBehaviour>();
        if (turret)
            turret.fireRateMultiplier -= bonusRate;
    }
}
