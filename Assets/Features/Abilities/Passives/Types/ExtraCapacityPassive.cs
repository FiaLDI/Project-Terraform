using UnityEngine;

[CreateAssetMenu(menuName = "Game/Passive/ExtraCapacity")]
public class ExtraCapacityPassive : PassiveSO
{
    public MaxEnergyBuffSO buff;
    private BuffInstance inst;

    public override void Apply(GameObject owner)
    {
        inst = owner.GetComponent<BuffSystem>().AddBuff(buff);
    }

    public override void Remove(GameObject owner)
    {
        if (inst != null)
            owner.GetComponent<BuffSystem>().RemoveBuff(inst);
    }
}
