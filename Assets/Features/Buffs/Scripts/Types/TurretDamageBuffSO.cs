using UnityEngine;

[CreateAssetMenu(menuName = "Game/Buff/Turret Damage")]
public class TurretDamageBuffSO : BuffSO
{
    public float bonus = 0.25f;

    public override string GetDescription()
    {
        return $"+{bonus * 100}% Turret Damage";
    }

    public override void OnApply(BuffInstance instance)
    {
        var turret = instance.Target.Transform.GetComponent<TurretBehaviour>();
        if (turret)
            turret.damageMultiplier += bonus;
    }

    public override void OnExpire(BuffInstance instance)
    {
        var turret = instance.Target.Transform.GetComponent<TurretBehaviour>();
        if (turret)
            turret.damageMultiplier -= bonus;
    }
}
