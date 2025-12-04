using System;
using System.Collections.Generic;
using UnityEngine;
using Features.Buffs.Domain;
using Features.Buffs.UnityIntegration;

namespace Features.Buffs.Application
{
    /// <summary>
    /// Бафф-сервис с защитой от дублирования, корректной работой стак-баффов
    /// и отменой повторного применения нестекаемых баффов.
    /// </summary>
    public class BuffService
    {
        private readonly BuffExecutor executor;

        /// <summary>
        /// Активные баффы. Один объект BuffInstance — один раз.
        /// </summary>
        private readonly List<BuffInstance> active = new();
        public IReadOnlyList<BuffInstance> Active => active;

        public event Action<BuffInstance> OnAdded;
        public event Action<BuffInstance> OnRemoved;

        public BuffService(BuffExecutor executor)
        {
            this.executor = executor;
        }

        // =====================================================================
        // ADD BUFF (главная логика)
        // =====================================================================
        public BuffInstance AddBuff(BuffSO config, IBuffTarget target)
        {
            if (config == null || target == null)
                return null;

            var existing = FindExisting(config, target);

            // --------------------------------------------------------------
            // 1) Бафф уже существует
            // --------------------------------------------------------------
            if (existing != null)
            {
                // --- STACKABLE ---
                if (config.isStackable)
                {
                    existing.StackCount++;
                    existing.Refresh();
                    executor.Apply(existing);       // применяем только если он стакается
                    OnAdded?.Invoke(existing);
                }
                else
                {
                    // --- NOT STACKABLE ---
                    // НЕ применяем повторно (ВАЖНО!)
                    // Просто продляем время
                    existing.Refresh();

                    // HUD всё равно нужно обновить
                    OnAdded?.Invoke(existing);
                }

                return existing;
            }

            // --------------------------------------------------------------
            // 2) Баффа нет → создаём новый
            // --------------------------------------------------------------
            var inst = new BuffInstance(config, target);
            active.Add(inst);

            executor.Apply(inst);               // первая активация
            OnAdded?.Invoke(inst);

            return inst;
        }

        // =====================================================================
        // FIND EXISTING (по СТАТУ и ТАРГЕТУ)
        // =====================================================================
        private BuffInstance FindExisting(BuffSO cfg, IBuffTarget target)
        {
            foreach (var inst in active)
            {
                // Сравниваем по stat, не по config!! иначе разные SO считаются разными.
                if (inst.Target == target &&
                    inst.Config.stat == cfg.stat &&
                    inst.Config.modType == cfg.modType)
                {
                    return inst;
                }
            }

            return null;
        }

        // =====================================================================
        // REMOVE
        // =====================================================================
        public void RemoveBuff(BuffInstance inst)
        {
            if (inst == null)
                return;

            if (active.Remove(inst))
            {
                executor.Expire(inst);
                OnRemoved?.Invoke(inst);
            }
        }

        // =====================================================================
        // TICK
        // =====================================================================
        public void Tick(float dt)
        {
            if (active.Count == 0)
                return;

            for (int i = active.Count - 1; i >= 0; i--)
            {
                var inst = active[i];

                executor.Tick(inst, dt);

                if (inst.IsExpired)
                {
                    executor.Expire(inst);
                    OnRemoved?.Invoke(inst);
                    active.RemoveAt(i);
                }
            }
        }

        // =====================================================================
        // CLEAR
        // =====================================================================
        public void ClearAll()
        {
            for (int i = active.Count - 1; i >= 0; i--)
            {
                executor.Expire(active[i]);
                OnRemoved?.Invoke(active[i]);
            }

            active.Clear();
        }
    }
}
