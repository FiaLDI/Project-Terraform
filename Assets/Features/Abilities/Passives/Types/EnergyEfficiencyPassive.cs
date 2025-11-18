using UnityEngine;

[CreateAssetMenu(menuName = "Game/Passive/EnergyEfficiency")]
public class EnergyEfficiencyPassive : PassiveSO
{
    public float radius = 15f;
    public float reductionPercent = 20f;

    public override void Apply(GameObject owner)
    {
        var aura = owner.AddComponent<PassiveAuraEmitter>();
        aura.radius = radius;
        aura.energyReductionPercent = reductionPercent;
    }

    public override void Remove(GameObject owner)
    {
        var aura = owner.GetComponent<PassiveAuraEmitter>();
        if (aura != null)
            GameObject.Destroy(aura);
    }
}
