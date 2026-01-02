// Assets/Features/Buffs/Scripts/Application/BuffSystem.cs
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

        private struct PendingBuff
        {
            public BuffSO cfg;
            public IBuffSource source;
            public BuffLifetimeMode mode;
        }

        private readonly List<BuffInstance> pending = new();
        private readonly List<PendingBuff> pendingAdd = new();

        private bool ready;

        // ================= PUBLIC API =================

        public bool ServiceReady => ready;

        public event Action OnServiceReady;
        public event Action<BuffInstance> OnBuffAdded;
        public event Action<BuffInstance> OnBuffRemoved;

        public IReadOnlyList<BuffInstance> Active => service?.Active;

        // =====================================================
        // LIFECYCLE
        // =====================================================

        private void Awake()
        {
            ResolveTarget();
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            BuffTickSystem.Register(this);
        }

        public override void OnStopServer()
        {
            BuffTickSystem.Unregister(this);
            base.OnStopServer();
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            ActiveBuffIds.OnChange += OnBuffIdsChanged;

            if (service == null)
                service = new BuffService();
        }

        public override void OnStopClient()
        {
            ActiveBuffIds.OnChange -= OnBuffIdsChanged;
            base.OnStopClient();
        }

        // =====================================================
        // INIT (POLLING — БЕЗ СОБЫТИЙ)
        // =====================================================

        private void ResolveTarget()
        {
            target =
                GetComponent<IBuffTarget>() ??
                GetComponentInChildren<IBuffTarget>() ??
                GetComponentInParent<IBuffTarget>();

            if (target == null)
                Debug.LogError("[BuffSystem] IBuffTarget not found!", this);
        }

        private void TryInitNow()
        {
            if (ready)
                return;

            if (target == null)
                return;

            executor = BuffExecutor.Instance;
            if (executor == null || !executor.IsServerStarted)
                return;

            if (target.GetServerStats() == null)
                return;

            service = new BuffService();
            service.OnAdded += HandleBuffAdded;
            service.OnRemoved += HandleBuffRemoved;

            ready = true;
            OnServiceReady?.Invoke();

            Debug.Log("[BuffSystem] READY (polling-safe)", this);

            foreach (var p in pendingAdd)
                service.AddBuff(p.cfg, target, p.source, p.mode);

            pendingAdd.Clear();
        }

        // =====================================================
        // SERVER API
        // =====================================================

        public BuffInstance Add(
            BuffSO cfg,
            IBuffSource source,
            BuffLifetimeMode lifetimeMode = BuffLifetimeMode.Duration)
        {
            if (!IsServerStarted || cfg == null)
                return null;

            TryInitNow();

            if (!ready)
            {
                if (debugMode)
                    Debug.Log($"[BuffSystem] Queue buff {cfg.buffId}", this);

                pendingAdd.Add(new PendingBuff
                {
                    cfg = cfg,
                    source = source,
                    mode = lifetimeMode
                });

                return null;
            }

            return service.AddBuff(cfg, target, source, lifetimeMode);
        }

        public void Remove(BuffInstance inst)
        {
            if (!IsServerStarted || inst == null || service == null)
                return;

            pending.Remove(inst);
            executor?.Expire(inst);
            service.RemoveBuff(inst);
        }

        public void RemoveBySource(IBuffSource source)
        {
            if (!IsServerStarted || service == null)
                return;

            pending.RemoveAll(b => b.Source == source);
            service.RemoveBySource(source);
        }

        public void ClearAll()
        {
            if (!IsServerStarted || service == null)
                return;

            foreach (var inst in pending)
                executor?.Expire(inst);

            pending.Clear();
            service.ClearAll();
        }

        // =====================================================
        // TICK
        // =====================================================

        public void Tick(float dt)
        {
            if (!IsServerStarted)
                return;

            if (!ready)
            {
                TryInitNow();
                return;
            }

            service.Tick(dt);
            TryApplyPending();
        }

        // =====================================================
        // APPLY
        // =====================================================

        private void HandleBuffAdded(BuffInstance inst)
        {
            OnBuffAdded?.Invoke(inst);

            if (!ApplyIfPossible(inst))
                pending.Add(inst);

            SyncActiveBuffs();
        }

        private void HandleBuffRemoved(BuffInstance inst)
        {
            pending.Remove(inst);
            executor?.Expire(inst);
            OnBuffRemoved?.Invoke(inst);
            SyncActiveBuffs();
        }

        private bool ApplyIfPossible(BuffInstance inst)
        {
            if (executor == null || inst == null)
                return false;

            executor.Apply(inst);
            Debug.Log($"[BUFF APPLY] {inst.Config.buffId}", this);
            return true;
        }

        private void TryApplyPending()
        {
            for (int i = pending.Count - 1; i >= 0; i--)
                if (ApplyIfPossible(pending[i]))
                    pending.RemoveAt(i);
        }

        // =====================================================
        // SYNC
        // =====================================================

        private void SyncActiveBuffs()
        {
            if (!IsServerStarted || service == null)
                return;

            var newIds = new HashSet<string>();
            foreach (var buff in service.Active)
                if (buff?.Config != null)
                    newIds.Add(buff.Config.buffId);

            for (int i = ActiveBuffIds.Count - 1; i >= 0; i--)
                if (!newIds.Contains(ActiveBuffIds[i]))
                    ActiveBuffIds.RemoveAt(i);

            foreach (var id in newIds)
                if (!ActiveBuffIds.Contains(id))
                    ActiveBuffIds.Add(id);
        }

        // =====================================================
        // CLIENT VIEW
        // =====================================================

        private void OnBuffIdsChanged(
            SyncListOperation op,
            int index,
            string oldItem,
            string newItem,
            bool asServer)
        {
            if (!asServer)
                SyncClientState();
        }

        private void SyncClientState()
        {
            if (service == null)
                return;

            var local = new List<BuffInstance>(service.Active);
            var serverIds = new HashSet<string>(ActiveBuffIds);

            foreach (var buff in local)
                if (buff?.Config != null &&
                    !serverIds.Contains(buff.Config.buffId))
                    service.RemoveBuff(buff);

            foreach (var id in serverIds)
            {
                bool exists = false;
                foreach (var b in service.Active)
                    if (b?.Config?.buffId == id)
                        exists = true;

                if (!exists)
                {
                    var cfg = BuffRegistrySO.Instance.GetById(id);
                    if (cfg != null)
                        service.AddBuff(cfg, target, null, BuffLifetimeMode.Duration);
                }
            }
        }
    }
}
