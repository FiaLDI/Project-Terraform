using UnityEngine;
using System;
using System.Collections.Generic;
using Features.Buffs.Domain;
using Features.Buffs.Application;
using Features.Stats.Domain;
using Features.Stats.UnityIntegration;

namespace Features.Buffs.UnityIntegration
{
    public class BuffExecutor : MonoBehaviour
    {
        // ============================================================
        // REGISTRY — сопоставление BuffStat → обработчика
        // ============================================================

        private readonly Dictionary<BuffStat, Action<BuffInstance, bool>> handlers =
            new Dictionary<BuffStat, Action<BuffInstance, bool>>();

        private void Awake()
        {
            RegisterDefaultHandlers();
        }

        // ============================================================
        // PUBLIC API — Apply / Expire / Tick
        // ============================================================

        public void Apply(BuffInstance inst)
        {
            if (!IsValid(inst)) return;

            var stat = inst.Config.stat;
            if (handlers.TryGetValue(stat, out var handler))
                handler.Invoke(inst, true);
        }

        public void Expire(BuffInstance inst)
        {
            if (!IsValid(inst)) return;

            var stat = inst.Config.stat;
            if (handlers.TryGetValue(stat, out var handler))
                handler.Invoke(inst, false);
        }

        public void Tick(BuffInstance inst, float dt)
        {
            if (!IsValid(inst)) return;

            var cfg = inst.Config;
            var go = inst.Target.GameObject;

            if (cfg.stat == BuffStat.HealPerSecond)
            {
                if (go.TryGetComponent<IHealthReceiver>(out var hp))
                    hp.Heal(cfg.value * dt);
            }
        }

        // ============================================================
        // VALIDATION
        // ============================================================

        private bool IsValid(BuffInstance inst)
        {
            return inst != null &&
                   inst.Config != null &&
                   inst.Target != null &&
                   inst.Target.GameObject != null;
        }

        // ============================================================
        // REGISTRATION OF HANDLERS
        // ============================================================

        private void RegisterDefaultHandlers()
        {
            // PLAYER COMBAT
            Register(BuffStat.PlayerDamage,
                (inst, apply) =>
                    TryCall<ICombatStatReceiver>(inst, r => r.ApplyCombatBuff(inst.Config, apply)));

            // MOVEMENT
            Register(BuffStat.PlayerMoveSpeed,
                (inst, apply) =>
                    TryCall<IMovementStatReceiver>(inst, r => r.ApplyMovementBuff(inst.Config, apply)));

            // MINING
            Register(BuffStat.PlayerMiningSpeed,
                (inst, apply) =>
                    TryCall<IMiningStatReceiver>(inst, r => r.ApplyMiningBuff(inst.Config, apply)));

            // SHIELD
            Register(BuffStat.PlayerShield,
                (inst, apply) =>
                    TryCall<IShieldReceiver>(inst, r => r.ApplyShieldBuff(inst.Config, apply)));

            // ENERGY
            Register(BuffStat.PlayerMaxEnergy,
                (inst, apply) =>
                    TryCall<IEnergyStatReceiver>(inst, r => r.ApplyEnergyBuff(inst.Config, apply)));

            Register(BuffStat.PlayerEnergyRegen,
                (inst, apply) =>
                    TryCall<IEnergyStatReceiver>(inst, r => r.ApplyEnergyBuff(inst.Config, apply)));

            Register(BuffStat.PlayerEnergyCostReduction,
                (inst, apply) =>
                    TryCall<IEnergyStatReceiver>(inst, r => r.ApplyEnergyBuff(inst.Config, apply)));

            // TURRET
            Register(BuffStat.TurretDamage,
                (inst, apply) =>
                    TryCall<ITurretStatReceiver>(inst, r => r.ApplyTurretBuff(inst.Config, apply)));

            Register(BuffStat.TurretFireRate,
                (inst, apply) =>
                    TryCall<ITurretStatReceiver>(inst, r => r.ApplyTurretBuff(inst.Config, apply)));

            Register(BuffStat.TurretRotationSpeed,
                (inst, apply) =>
                    TryCall<ITurretStatReceiver>(inst, r => r.ApplyTurretBuff(inst.Config, apply)));

            Register(BuffStat.TurretMaxHP,
                (inst, apply) =>
                    TryCall<ITurretStatReceiver>(inst, r => r.ApplyTurretBuff(inst.Config, apply)));

            // UNIVERSAL — HealPerSecond handled in Tick()
            // (не регистрируем)
        }

        // ============================================================
        // HELPERS
        // ============================================================

        private void Register(BuffStat stat, Action<BuffInstance, bool> handler)
        {
            handlers[stat] = handler;
        }

        /// <summary>
        /// Унифицированный вызов интерфейса с TryGetComponent.
        /// </summary>
        private void TryCall<T>(BuffInstance inst, Action<T> call)
        {
            var go = inst.Target.GameObject;

            // 🔥 приоритет: StatBuffReceiver
            if (go.TryGetComponent<StatBuffReceiver>(out var statRecv) && statRecv is T t1)
            {
                call(t1);
                return;
            }

            // 🔥 fallback: старые системы (например, Turret)
            if (go.TryGetComponent<T>(out var receiver))
                call(receiver);
        }

    }
}
