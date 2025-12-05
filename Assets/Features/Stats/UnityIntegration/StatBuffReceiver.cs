using UnityEngine;
using Features.Buffs.Domain;
using Features.Stats.Domain;

namespace Features.Stats.UnityIntegration
{
    [DefaultExecutionOrder(-450)]
    public class StatBuffReceiver : MonoBehaviour,
        ICombatStatReceiver,
        IMovementStatReceiver,
        IMiningStatReceiver,
        IShieldReceiver,
        IEnergyStatReceiver,
        IHealthStatReceiver,
        ITurretStatReceiver
    {
        private IStatsFacade stats;

        // вызывается ИЗ PlayerClassController
        public void Init(IStatsFacade stats)
        {
            this.stats = stats;
        }

        private bool Ready()
        {
            return stats != null;
        }

        public void ApplyCombatBuff(BuffSO cfg, bool apply)
        {
            if (!Ready()) return;
            stats.Combat.ApplyBuff(cfg, apply);
        }

        public void ApplyMovementBuff(BuffSO cfg, bool apply)
        {
            if (!Ready()) return;
            stats.Movement.ApplyBuff(cfg, apply);
        }

        public void ApplyMiningBuff(BuffSO cfg, bool apply)
        {
            if (!Ready()) return;
            stats.Mining.ApplyBuff(cfg, apply);
        }

        public void ApplyShieldBuff(BuffSO cfg, bool apply)
        {
            if (!Ready()) return;
            stats.Health.ApplyBuff(cfg, apply);
        }

        public void ApplyEnergyBuff(BuffSO cfg, bool apply)
        {
            if (!Ready()) return;
            stats.Energy.ApplyBuff(cfg, apply);
        }

        public void ApplyHealthBuff(BuffSO cfg, bool apply)
        {
            if (!Ready()) return;
            stats.Health.ApplyBuff(cfg, apply);
        }

        public void ApplyTurretBuff(BuffSO cfg, bool apply)
        {
            stats.Combat.ApplyBuff(cfg, apply);
            stats.Movement.ApplyBuff(cfg, apply);
            stats.Health.ApplyBuff(cfg, apply);
        }
    }
}
