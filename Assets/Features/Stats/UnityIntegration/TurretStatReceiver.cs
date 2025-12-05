using UnityEngine;
using Features.Buffs.Domain;
using Features.Stats.Domain;
using Features.Stats.UnityIntegration;

public class TurretStatReceiver : MonoBehaviour, ITurretStatReceiver
{
    private TurretStats stats;

    private void Awake()
    {
        stats = GetComponent<TurretStats>();
    }

    public void ApplyTurretBuff(BuffSO cfg, bool apply)
    {
        float sign = apply ? 1f : -1f;

        switch (cfg.stat)
        {
            case BuffStat.TurretDamage:
                stats.DamageBonusMult += (cfg.modType == BuffModType.Mult ? (cfg.value - 1f) * sign : 0);
                stats.DamageBonusAdd += (cfg.modType == BuffModType.Add ? cfg.value * sign : 0);
                break;

            case BuffStat.TurretFireRate:
                stats.FireRateMultiplier *= apply ? cfg.value : 1f / cfg.value;
                break;

            case BuffStat.TurretRotationSpeed:
                stats.RotationBonusMult *= apply ? cfg.value : 1f / cfg.value;
                break;

            case BuffStat.TurretMaxHP:
                stats.MaxHpBonus += cfg.value * sign;
                stats.RecalculateHP();
                break;
        }
    }
}
