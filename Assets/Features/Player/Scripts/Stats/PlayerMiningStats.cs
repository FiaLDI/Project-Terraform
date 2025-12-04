using UnityEngine;
using Features.Buffs.Domain;
using Features.Stats.Domain;

public class PlayerMiningStats : MonoBehaviour, IMiningStatReceiver
{
    [SerializeField] private float baseMultiplier = 1f;

    private float addBonus = 0f;
    private float multBonus = 1f;

    public float MiningMultiplier => (baseMultiplier + addBonus) * multBonus;

    public void ApplyMiningBuff(BuffSO cfg, bool apply)
    {
        switch (cfg.modType)
        {
            case BuffModType.Add:
                addBonus += apply ? cfg.value : -cfg.value;
                break;

            case BuffModType.Mult:
                if (apply) multBonus *= cfg.value;
                else if (cfg.value != 0f) multBonus /= cfg.value;
                break;

            case BuffModType.Set:
                multBonus = apply ? cfg.value : 1f;
                break;
        }

        if (addBonus < 0)
            addBonus = 0;
    }

    public void ApplyMiningSpeedBuff(BuffSO cfg, bool apply)
        => ApplyMiningBuff(cfg, apply);


    public void SetBase(float v)
        => baseMultiplier = v;
}
