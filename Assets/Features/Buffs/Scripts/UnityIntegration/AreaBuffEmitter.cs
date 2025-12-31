using UnityEngine;
using System.Collections.Generic;
using Features.Buffs.Domain;
using FishNet.Object;
using Features.Buffs.Application;

namespace Features.Buffs.UnityIntegration
{
    /// <summary>
    /// Серверный эмиттер ауры.
    /// НЕ тикает сам — вызывается BuffTickSystem.
    ///
    /// Архитектура:
    /// - сам является BuffSource
    /// - баффы живут пока цель внутри радиуса
    /// - удаление строго через RemoveBySource
    /// </summary>
    public sealed class AreaBuffEmitter : NetworkBehaviour, IBuffSource
    {
        [Header("Config")]
        public AreaBuffSO area;
        private bool _serverActive;

        // Все цели, которые сейчас внутри ауры
        private readonly HashSet<IBuffTarget> inside = new();

        public override void OnStartServer()
        {
            base.OnStartServer();
            _serverActive = true;
            BuffTickSystem.RegisterEmitter(this);
        }

        public override void OnStopServer()
        {
            _serverActive = false;
            CleanupAll();
            BuffTickSystem.UnregisterEmitter(this);
            base.OnStopServer();
        }

        // =====================================================
        // SERVER TICK (ВЫЗЫВАЕТ BuffTickSystem)
        // =====================================================

        public void Tick()
        {
            if (!IsServerStarted || area == null || area.buff == null)
                return;

            Collider[] hits = Physics.OverlapSphere(
                transform.position,
                area.radius,
                area.targetMask
            );

            var current = new HashSet<IBuffTarget>();

            foreach (var h in hits)
            {
                if (!h.TryGetComponent(out IBuffTarget target))
                    continue;

                current.Add(target);

                // ➕ ВХОД В АУРУ
                if (!inside.Contains(target))
                {
                    var bs = target.BuffSystem;
                    if (bs != null)
                    {
                        bs.Add(
                            area.buff,
                            source: this,
                            lifetimeMode: BuffLifetimeMode.WhileSourceAlive
                        );
                    }
                }
            }

            // ➖ ВЫХОД ИЗ АУРЫ
            foreach (var target in inside)
            {
                if (!current.Contains(target))
                {
                    var bs = target?.BuffSystem;
                    if (bs != null)
                        bs.RemoveBySource(this);
                }
            }

            inside.Clear();
            inside.UnionWith(current);
        }

        // =====================================================
        // CLEANUP
        // =====================================================

        private void CleanupAll()
        {
            foreach (var target in inside)
            {
                var bs = target?.BuffSystem;
                if (bs != null)
                    bs.RemoveBySource(this);
            }

            inside.Clear();
        }

        private void OnDestroy()
        {
            if (_serverActive)
                CleanupAll();
        }
    }
}
