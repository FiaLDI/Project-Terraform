using UnityEngine;

[CreateAssetMenu(menuName = "Game/Buff/Mining/Mining Speed Boost")]
public class MiningSpeedBoostBuffSO : BuffSO
{
    [Header("Mining Speed Multiplier")]
    public float multiplier = 1.3f;  // +30%

    public override void OnApply(BuffInstance instance)
    {
        var mod = new GlobalStatModifier
        {
            stat = ItemStatType.MiningSpeed,
            flatBonus = 0f,
            percentBonus = multiplier - 1f
        };

        instance.AddGlobalModifier(mod);
    }

    public override void OnExpire(BuffInstance instance)
    {
        instance.RemoveAllGlobalModifiers();
    }
}
