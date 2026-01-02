using UnityEngine;
using System;
using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Features.Buffs.Domain;
using Features.Buffs.Data;
using Features.Buffs.UnityIntegration;

namespace Features.Buffs.Application
{
    public sealed class BuffSystem : NetworkBehaviour
    {
        [Header("Debug")]
        [SerializeField] private bool debugMode;

        // ================= NETWORK =================

        public readonly SyncList<string> ActiveBuffIds = new();

        // ================= RUNTIME =================

        private IBuffTarget target;
        private BuffExecutor executor;
        private BuffService service;
        private ServerGamePhase phase;

        private bool ready;

        // ================= PUBLIC =================

        public bool ServiceReady => ready;
        public event Action OnServiceReady;

        public IReadOnlyList<BuffInstance> Active => service?.Active;

        // =====================================================
        // LIFECYCLE
        // =====================================================

        private void Awake()
        {
            ResolveTarget();
            phase = GetComponent<ServerGamePhase>();
        }

        public override void OnStartServer()
        {
            base.OnStartServer();

            BuffTickSystem.Register(this);

            if (phase == null)
            {
                Debug.LogError("[BuffSystem] ServerGamePhase missing", this);
                return;
            }

            phase.OnPhaseReached += OnPhaseReached;
        }

        public override void OnStopServer()
        {
            BuffTickSystem.Unregister(this);

            if (phase != null)
                phase.OnPhaseReached -= OnPhaseReached;

            base.OnStopServer();
        }

        // =====================================================
        // PHASE
        // =====================================================

        private void OnPhaseReached(GamePhase p)
        {
            if (p == GamePhase.StatsReady)
                InitService();
        }

        private void InitService()
        {
            if (ready)
                return;

            executor = BuffExecutor.Instance;
            if (executor == null)
            {
                Debug.LogError("[BuffSystem] BuffExecutor not ready", this);
                return;
            }

            if (target.GetServerStats() == null)
            {
                Debug.LogError("[BuffSystem] Stats not ready", this);
                return;
            }

            service = new BuffService();
            service.OnAdded += HandleBuffAdded;
            service.OnRemoved += HandleBuffRemoved;

            ready = true;

            Debug.Log("[BuffSystem] READY", this);

            OnServiceReady?.Invoke();
            phase.Reach(GamePhase.BuffsReady);
        }

        // =====================================================
        // SERVER API
        // =====================================================

        public BuffInstance Add(
            BuffSO cfg,
            IBuffSource source,
            BuffLifetimeMode lifetimeMode = BuffLifetimeMode.Duration)
        {
            if (!PhaseAssert.Require(phase, GamePhase.BuffsReady, this))
                return null;
            if (!IsServer || !ready || cfg == null)
            {
                if (debugMode)
                    Debug.Log($"[BuffSystem] Reject Add {cfg?.buffId}", this);
                return null;
            }

            return service.AddBuff(cfg, target, source, lifetimeMode);
        }

        public void Remove(BuffInstance inst)
        {
            if (!IsServer || !ready || inst == null)
                return;

            executor.Expire(inst);
            service.RemoveBuff(inst);
        }

        public void RemoveBySource(IBuffSource source)
        {
            if (!IsServer || !ready)
                return;

            service.RemoveBySource(source);
        }

        public void ClearAll()
        {
            if (!IsServer || !ready)
                return;

            service.ClearAll();
        }

        // =====================================================
        // TICK
        // =====================================================

        public void Tick(float dt)
        {
            if (!IsServer || !ready)
                return;

            service.Tick(dt);
        }

        // =====================================================
        // INTERNAL
        // =====================================================

        private void ResolveTarget()
        {
            target =
                GetComponent<IBuffTarget>() ??
                GetComponentInChildren<IBuffTarget>() ??
                GetComponentInParent<IBuffTarget>();

            if (target == null)
                Debug.LogError("[BuffSystem] IBuffTarget not found", this);
        }

        private void HandleBuffAdded(BuffInstance inst)
        {
            executor.Apply(inst);
            SyncActiveBuffs();
        }

        private void HandleBuffRemoved(BuffInstance inst)
        {
            executor.Expire(inst);
            SyncActiveBuffs();
        }

        private void SyncActiveBuffs()
        {
            if (!IsServer || service == null)
                return;

            var ids = new HashSet<string>();
            foreach (var b in service.Active)
                if (b?.Config != null)
                    ids.Add(b.Config.buffId);

            ActiveBuffIds.Clear();
            foreach (var id in ids)
                ActiveBuffIds.Add(id);
        }
    }
}
