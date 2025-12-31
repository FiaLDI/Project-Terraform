using UnityEngine;
using System.Collections.Generic;
using FishNet.Object;
using Features.Buffs.Domain;
using Features.Buffs.Application;

namespace Features.Buffs.UnityIntegration
{
    /// <summary>
    /// Серверный эмиттер ауры (DISTANCE-BASED).
    ///
    /// ✔ Без Physics
    /// ✔ Без LayerMask
    /// ✔ Без Collider
    /// ✔ Работает одинаково для host и клиентов
    /// ✔ Единственный источник истины — сервер
    /// </summary>
    public sealed class AreaBuffEmitter : NetworkBehaviour, IBuffSource
    {
        [Header("Config")]
        public AreaBuffSO area;

        private readonly HashSet<IBuffTarget> inside = new();
        private bool _active;

        public override void OnStartServer()
        {
            base.OnStartServer();

            if (area == null || area.buff == null)
            {
                Debug.LogError("[AreaBuffEmitter] Area or Buff is null", this);
                return;
            }

            _active = true;
            BuffTickSystem.RegisterEmitter(this);
        }

        public override void OnStopServer()
        {
            _active = false;
            CleanupAll();
            BuffTickSystem.UnregisterEmitter(this);
            base.OnStopServer();
        }

        private void OnDestroy()
        {
            if (_active)
                CleanupAll();
        }

        // =====================================================
        // SERVER TICK
        // =====================================================

        public void Tick()
        {
            if (!_active || !IsServerStarted || area == null || area.buff == null)
                return;

            Vector3 pos = transform.position;
            float radiusSqr = area.radius * area.radius;

            var current = new HashSet<IBuffTarget>();

            foreach (var target in ServerBuffTargetRegistry.All)
            {
                if (target == null)
                    continue;

                if (!(target is Component comp))
                    continue;

                float distSqr =
                    (comp.transform.position - pos).sqrMagnitude;

                if (distSqr > radiusSqr)
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
    }
}
