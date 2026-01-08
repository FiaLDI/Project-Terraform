using UnityEngine;
using System.Collections.Generic;
using FishNet.Object;
using Features.Buffs.Domain;
using Features.Buffs.Application;

namespace Features.Buffs.UnityIntegration
{
    public sealed class AreaBuffEmitter : NetworkBehaviour, IBuffSource
    {
        [Header("Config")]
        public AreaBuffSO area;
        
        private readonly HashSet<int> inside = new();
        private readonly HashSet<IBuffTarget> current = new();

        private bool _active;

        // =====================================================
        // SERVER LIFECYCLE
        // =====================================================

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
            if (_active)
            {
                CleanupAll();
                BuffTickSystem.UnregisterEmitter(this);
                _active = false;
            }

            base.OnStopServer();
        }

        private void OnDestroy()
        {
            if (_active)
            {
                CleanupAll();
                _active = false;
            }
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

            var current = new HashSet<int>();

            foreach (var target in ServerBuffTargetRegistry.All)
            {
                if (target == null)
                    continue;

                var stats = target.GetServerStats();
                if (stats == null || target.BuffSystem == null)
                    continue;

                if (target is not Component comp)
                    continue;

                if (!comp.TryGetComponent<NetworkObject>(out var no))
                    continue;

                int id = no.ObjectId;

                if ((comp.transform.position - pos).sqrMagnitude > radiusSqr)
                    continue;

                current.Add(id);

                if (!inside.Contains(id))
                {
                    target.BuffSystem.Add(
                        area.buff,
                        source: this,
                        lifetimeMode: BuffLifetimeMode.WhileSourceAlive
                    );
                }
            }

            // ➖ ВЫХОД ИЗ АУРЫ
            foreach (var oldId in inside)
            {
                if (!current.Contains(oldId))
                {
                    var target = ServerBuffTargetRegistry.FindByNetId(oldId);
                    target?.BuffSystem?.RemoveBySource(this);
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
            foreach (var netId in inside)
            {
                var target = ServerBuffTargetRegistry.FindByNetId(netId);
                var bs = target?.BuffSystem;

                if (bs != null)
                    bs.RemoveBySource(this);
            }

            inside.Clear();
        }

    }
}
