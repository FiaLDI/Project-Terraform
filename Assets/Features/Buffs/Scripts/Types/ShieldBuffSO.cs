using UnityEngine;

[CreateAssetMenu(menuName = "Game/Buff/Shield")]
public class ShieldBuffSO : BuffSO
{
    public float shieldAmount = 50f;

    public override string GetDescription()
    {
        return $"+{shieldAmount} Shield";
    }

    public override void OnApply(BuffInstance instance)
    {
        var hp = instance.Target.Transform.GetComponent<PlayerHealth>();
        if (hp)
            hp.AddShield(shieldAmount);
    }

    public override void OnExpire(BuffInstance instance)
    {
        var hp = instance.Target.Transform.GetComponent<PlayerHealth>();
        if (hp)
            hp.RemoveShield(shieldAmount);
    }
}
