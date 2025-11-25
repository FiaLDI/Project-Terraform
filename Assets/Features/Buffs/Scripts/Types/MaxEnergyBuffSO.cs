using UnityEngine;

[CreateAssetMenu(menuName = "Game/Buff/Max Energy")]
public class MaxEnergyBuffSO : BuffSO
{
    public float extraMaxEnergy = 30f;

    public override void OnApply(BuffInstance instance)
    {
        if (instance.Target.GameObject.TryGetComponent<PlayerEnergy>(out var energy))
        {
            energy.SetMaxEnergy(energy.MaxEnergy + extraMaxEnergy, false);
        }
    }

    public override void OnExpire(BuffInstance instance)
    {
        if (instance.Target.GameObject.TryGetComponent<PlayerEnergy>(out var energy))
        {
            energy.SetMaxEnergy(energy.MaxEnergy - extraMaxEnergy, false);
        }
    }
}
