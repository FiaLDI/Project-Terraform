using UnityEngine;
using Features.Buffs.Domain;
using Features.Buffs.Application;
using Features.Combat.Devices;
using Features.Combat.Actors;

namespace Features.Buffs.UnityIntegration
{
    public class BuffExecutor : MonoBehaviour
    {
        public void Apply(BuffInstance inst)
        {
            BuffSO cfg = inst.Config;
            GameObject target = inst.Target.GameObject;

            switch (cfg.stat)
            {
                // PLAYER -----------------------------
                case BuffStat.PlayerDamage:
                    ModifyMultiplier<PlayerCombat>(target, "damageMultiplier", cfg, true);
                    break;

                case BuffStat.PlayerMoveSpeed:
                    ModifyMultiplier<PlayerMovementStats>(target, "speedMultiplier", cfg, true);
                    break;

                case BuffStat.PlayerShield:
                    if (target.TryGetComponent<PlayerHealth>(out var hp))
                        hp.AddShield(cfg.value);
                    break;

                case BuffStat.PlayerMaxEnergy:
                    if (target.TryGetComponent<PlayerEnergy>(out var peMax))
                        peMax.SetMaxEnergy(peMax.MaxEnergy + cfg.value, fill:false);
                    break;

                case BuffStat.PlayerEnergyCostReduction:
                    if (target.TryGetComponent<PlayerEnergy>(out var peCR))
                        peCR.AddCostReduction(cfg.value);
                    break;

                case BuffStat.PlayerEnergyRegen:
                    if (target.TryGetComponent<PlayerEnergy>(out var peR))
                        peR.AddRegenPercent(cfg.value);
                    break;

                case BuffStat.PlayerMiningSpeed:
                    ModifyMultiplier<PlayerMiningStats>(target, "miningMultiplier", cfg, true);
                    break;


                // TURRET ------------------------------
                case BuffStat.TurretDamage:
                    ModifyMultiplier<TurretBehaviour>(target, "damageMultiplier", cfg, true);
                    break;

                case BuffStat.TurretFireRate:
                    ModifyMultiplier<TurretBehaviour>(target, "fireRateMultiplier", cfg, true);
                    break;

                case BuffStat.TurretRotationSpeed:
                    ModifyMultiplier<TurretBehaviour>(target, "rotationSpeedMultiplier", cfg, true);
                    break;

                case BuffStat.TurretMaxHP:
                    if (target.TryGetComponent<HealthComponent>(out var thp))
                    {
                        thp.maxHp += cfg.value;
                        thp.currentHp += cfg.value; // чтобы HP остался пропорциональным
                        thp.OnHealthChanged?.Invoke(thp.currentHp, thp.maxHp);
                    }
                    break;


                // UNIVERSAL ---------------------------
                case BuffStat.HealPerSecond:
                    // handled in Tick
                    break;
            }
        }


        public void Expire(BuffInstance inst)
        {
            BuffSO cfg = inst.Config;
            GameObject target = inst.Target.GameObject;

            switch (cfg.stat)
            {
                // PLAYER
                case BuffStat.PlayerDamage:
                    ModifyMultiplier<PlayerCombat>(target, "damageMultiplier", cfg, false);
                    break;

                case BuffStat.PlayerMoveSpeed:
                    ModifyMultiplier<PlayerMovementStats>(target, "speedMultiplier", cfg, false);
                    break;

                case BuffStat.PlayerShield:
                    if (target.TryGetComponent<PlayerHealth>(out var hp))
                        hp.RemoveShield(cfg.value);
                    break;

                case BuffStat.PlayerMaxEnergy:
                    if (target.TryGetComponent<PlayerEnergy>(out var peMax))
                        peMax.SetMaxEnergy(peMax.MaxEnergy - cfg.value, fill:false);
                    break;

                case BuffStat.PlayerEnergyCostReduction:
                    if (target.TryGetComponent<PlayerEnergy>(out var peCR))
                        peCR.RemoveCostReduction(cfg.value);
                    break;

                case BuffStat.PlayerEnergyRegen:
                    if (target.TryGetComponent<PlayerEnergy>(out var peR))
                        peR.RemoveRegenPercent(cfg.value);
                    break;

                case BuffStat.PlayerMiningSpeed:
                    ModifyMultiplier<PlayerMiningStats>(target, "miningMultiplier", cfg, false);
                    break;


                // TURRET
                case BuffStat.TurretDamage:
                    ModifyMultiplier<TurretBehaviour>(target, "damageMultiplier", cfg, false);
                    break;

                case BuffStat.TurretFireRate:
                    ModifyMultiplier<TurretBehaviour>(target, "fireRateMultiplier", cfg, false);
                    break;

                case BuffStat.TurretRotationSpeed:
                    ModifyMultiplier<TurretBehaviour>(target, "rotationSpeedMultiplier", cfg, false);
                    break;

                case BuffStat.TurretMaxHP:
                    if (target.TryGetComponent<HealthComponent>(out var thp))
                    {
                        thp.maxHp -= cfg.value;

                        if (thp.currentHp > thp.maxHp)
                            thp.currentHp = thp.maxHp;

                        thp.OnHealthChanged?.Invoke(thp.currentHp, thp.maxHp);
                    }
                    break;

            }
        }


        public void Tick(BuffInstance inst, float dt)
        {
            if (inst.Config.stat == BuffStat.HealPerSecond)
            {
                var t = inst.Target.GameObject;
                if (t.TryGetComponent<PlayerHealth>(out var hp))
                    hp.Heal(inst.Config.value * dt);
            }
        }

        private void ModifyMultiplier<T>(GameObject target, string field, BuffSO cfg, bool apply)
        {
            if (target.TryGetComponent(typeof(T), out var comp))
            {
                var f = comp.GetType().GetField(field);
                if (f == null) return;

                float val = (float)f.GetValue(comp);

                switch (cfg.modType)
                {
                    case BuffModType.Add:
                        val += apply ? cfg.value : -cfg.value;
                        break;

                    case BuffModType.Mult:
                        val = apply ? val * cfg.value : val / cfg.value;
                        break;

                    case BuffModType.Set:
                        val = apply ? cfg.value : 1f; // сброс к дефолту
                        break;
                }

                f.SetValue(comp, val);
            }
        }
    }
}
