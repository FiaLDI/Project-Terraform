using UnityEngine;

[CreateAssetMenu(menuName = "Game/Passive/ExtraCapacity")]
public class ExtraCapacityPassive : PassiveSO
{
    public float bonusMaxEnergy = 40f;

    private BuffInstance activeBuff;

    public override void Apply(GameObject owner)
    {
        var buffs = owner.GetComponent<BuffSystem>();
        if (buffs != null)
        {
            activeBuff = buffs.AddBuff(
                BuffType.MaxEnergy,
                bonusMaxEnergy,
                Mathf.Infinity, // пассивка = вечный баф
                icon
            );
        }
    }

    public override void Remove(GameObject owner)
    {
        if (activeBuff == null) return;

        var buffs = owner.GetComponent<BuffSystem>();
        if (buffs != null)
        {
            buffs.RemoveBuffInstance(activeBuff);
        }
    }
}
