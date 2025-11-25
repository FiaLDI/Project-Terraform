using UnityEngine;

[CreateAssetMenu(menuName = "Game/Passive/ReinforcedConstructions")]
public class ReinforcedConstructionsPassive : PassiveSO
{
    public GlobalBuffSO globalBuff;

    public override void Apply(GameObject owner)
    {
        GlobalBuffSystem.I.Add(globalBuff);
    }

    public override void Remove(GameObject owner)
    {
        GlobalBuffSystem.I.Remove(globalBuff);
    }
}
