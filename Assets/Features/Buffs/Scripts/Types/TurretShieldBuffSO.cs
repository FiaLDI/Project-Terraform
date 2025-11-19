using UnityEngine;

[CreateAssetMenu(menuName = "Game/Buff/Turret/Shield")]
public class TurretShieldBuffSO : BuffSO
{
    public float shieldAmount = 40f;

    public override void OnApply(BuffInstance instance)
    {
        if (instance.Target.GameObject.TryGetComponent<TurretBehaviour>(out var turret))
            turret.Heal(shieldAmount); // или отдельное поле shield
    }

    public override void OnExpire(BuffInstance instance)
    {
        // Турель может превышать HP, поэтому не снимаем
        // Если нужен реальный "щит" — скажи, сделаю
    }
}
