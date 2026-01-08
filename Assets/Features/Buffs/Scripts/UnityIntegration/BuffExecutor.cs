// Assets/Features/Buffs/Scripts/Application/BuffExecutor.cs
using UnityEngine;
using System;
using System.Collections.Generic;
using Features.Buffs.Domain;
using Features.Stats.Domain;

namespace Features.Buffs.Application
{
    /// <summary>
    /// ГЛОБАЛЬНЫЙ исполнитель баффов.
    /// ЕДИНСТВЕННЫЙ источник изменения статов.
    ///
    /// ❗ Не NetworkBehaviour
    /// ❗ Не спавнится
    /// ❗ Инициализируется ДО игроков
    /// </summary>
    public sealed class BuffExecutor : MonoBehaviour
    {
        private readonly Dictionary<BuffStat, Action<BuffInstance, IStatsFacade, bool>> handlers = new();

        public static BuffExecutor Instance { get; private set; }
        public static bool IsReady => Instance != null;

        // =====================================================
        // LIFECYCLE
        // =====================================================

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            transform.SetParent(null);
            Instance = this;
            DontDestroyOnLoad(gameObject);

            RegisterHandlers();

            Debug.Log("[BuffExecutor] READY (global)");
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;

            handlers.Clear();
        }

        // =====================================================
        // APPLY
        // =====================================================

        public bool Apply(BuffInstance inst)
        {
            if (!IsValid(inst))
                return false;

            var stats = inst.Target.GetServerStats();
            if (stats == null)
                return false;

            if (!handlers.TryGetValue(inst.Config.stat, out var h))
                return false;

            h(inst, stats, true);
            return true;
        }

        // =====================================================
        // EXPIRE
        // =====================================================

        public void Expire(BuffInstance inst)
        {
            if (!IsValid(inst))
                return;

            var stats = inst.Target.GetServerStats();
            if (stats == null)
                return;

            if (handlers.TryGetValue(inst.Config.stat, out var h))
                h(inst, stats, false);
        }

        // =====================================================
        // TICK (HoT / DoT)
        // =====================================================

        public void Tick(BuffInstance inst, float dt)
        {
            if (!IsValid(inst))
                return;

            var stats = inst.Target.GetServerStats();
            if (stats == null)
                return;

            if (inst.Config.stat == BuffStat.HealPerSecond)
            {
                stats.Health.Heal(inst.Config.value * dt);
            }
        }

        // =====================================================
        // VALIDATION
        // =====================================================

        private static bool IsValid(BuffInstance inst)
        {
            if (inst == null) return false;
            if (inst.Config == null) return false;
            if (inst.Target == null) return false;
            if (inst.Target.GameObject == null) return false;
            return true;
        }

        // =====================================================
        // HANDLERS REGISTRATION
        // =====================================================

        private void RegisterHandlers()
        {
            // ================= PLAYER =================

            Register(
                BuffStat.PlayerDamage,
                (i, s, apply) => s.Combat.ApplyBuff(i.Config, apply)
            );

            Register(
                BuffStat.PlayerMoveSpeed,
                (i, s, apply) => s.Movement.ApplyBuff(i.Config, apply)
            );

            Register(
                BuffStat.PlayerMoveSpeedMult,
                (i, s, apply) => s.Movement.ApplyBuff(i.Config, apply)
            );

            Register(
                BuffStat.PlayerHp,
                (i, s, apply) => s.Health.ApplyBuff(i.Config, apply)
            );

            Register(
                BuffStat.PlayerHpRegen,
                (i, s, apply) => s.Health.ApplyBuff(i.Config, apply)
            );

            Register(
                BuffStat.PlayerEnergyRegen,
                (i, s, apply) => s.Energy.ApplyBuff(i.Config, apply)
            );

            Register(
                BuffStat.PlayerMaxEnergy,
                (i, s, apply) => s.Energy.ApplyBuff(i.Config, apply)
            );

            // ================= TURRET =================

            Register(
                BuffStat.TurretDamage,
                (i, s, apply) => s.Combat.ApplyBuff(i.Config, apply)
            );

            Register(
                BuffStat.TurretFireRate,
                (i, s, apply) =>
                {
                    if (s.Combat is ITurretCombatStats tc)
                        tc.ApplyFireRateBuff(i.Config, apply);
                }
            );
        }

        private void Register(
            BuffStat stat,
            Action<BuffInstance, IStatsFacade, bool> handler)
        {
            handlers[stat] = handler;
        }
    }
}
