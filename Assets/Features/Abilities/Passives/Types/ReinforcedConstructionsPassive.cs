using UnityEngine;

[CreateAssetMenu(menuName = "Game/Passive/ReinforcedConstructions")]
public class ReinforcedConstructionsPassive : PassiveSO
{
    public float hpBonusPercent = 15f;

    public override void Apply(GameObject owner)
    {
        TurretBehaviour.GlobalHpBonusPercent = hpBonusPercent;
    }

    public override void Remove(GameObject owner)
    {
        TurretBehaviour.GlobalHpBonusPercent = 0f;
    }
}
