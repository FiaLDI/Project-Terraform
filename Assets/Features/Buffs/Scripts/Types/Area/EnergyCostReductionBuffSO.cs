using UnityEngine;

[CreateAssetMenu(menuName = "Game/Buff/Energy Cost Reduction")]
public class EnergyCostReductionBuffSO : BuffSO
{
    public float costReductionPercent = 20f;  // e.g. 20% cheaper

    public override void OnApply(BuffInstance instance)
    {
        if (instance.Target.GameObject.TryGetComponent<PlayerEnergy>(out var energy))
        {
            energy.AddCostReduction(costReductionPercent);
        }
    }

    public override void OnExpire(BuffInstance instance)
    {
        if (instance.Target.GameObject.TryGetComponent<PlayerEnergy>(out var energy))
        {
            energy.RemoveCostReduction(costReductionPercent);
        }
    }
}
