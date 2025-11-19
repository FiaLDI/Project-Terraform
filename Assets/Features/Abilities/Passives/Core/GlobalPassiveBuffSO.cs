using UnityEngine;

[CreateAssetMenu(menuName = "Game/Passive/Global Passive")]
public class GlobalPassiveBuffSO : PassiveSO
{
    [Header("Turret HP Bonus")]
    public float turretHpPercent = 0f;

    [Header("Ability Energy Cost Mult")]
    public float abilityEnergyMult = 1f;

    public override void Apply(GameObject owner)
    {
        TurretBehaviour.GlobalHpBonusPercent += turretHpPercent;
        AbilityCostSystem.GlobalCostMultiplier *= abilityEnergyMult;
    }

    public override void Remove(GameObject owner)
    {
        TurretBehaviour.GlobalHpBonusPercent -= turretHpPercent;
        AbilityCostSystem.GlobalCostMultiplier /= abilityEnergyMult;
    }
}
