using UnityEngine;
using System.Collections.Generic;
using Features.Buffs.Domain;
using FishNet.Object;
using FishNet.Object.Synchronizing;
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

        public bool ServiceReady => service != null;
        public IReadOnlyList<BuffInstance> Active => service?.Active;

        public event System.Action<BuffInstance> OnBuffAdded;
        public event System.Action<BuffInstance> OnBuffRemoved;

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

            TryInitServer();
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
            TryInitClient();
        }

        public override void OnStopClient()
        {
            ActiveBuffIds.OnChange -= OnBuffIdsChanged;
            base.OnStopClient();
        }

        // =====================================================
        // TICK (SERVER ONLY)
        // =====================================================

        public void Tick(float dt)
        {
            if (!IsServerStarted || service == null)
                return;

            service.Tick(dt);
        }

        // =====================================================
        // INIT
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

        // ---------- SERVER INIT ----------

        private void TryInitServer()
        {
            if (service != null)
                return;

            if (target == null)
                ResolveTarget();

            if (target == null)
                return;

            executor = BuffExecutor.Instance;
            if (executor == null || !executor.IsServerStarted)
            {
                if (debugMode)
                    Debug.Log("[BuffSystem] Waiting for server BuffExecutor", this);
                return;
            }

            service = new BuffService(executor);
            service.OnAdded += HandleBuffAdded;
            service.OnRemoved += HandleBuffRemoved;

            if (debugMode)
                Debug.Log("[BuffSystem] Server service initialized", this);
        }

        // ---------- CLIENT INIT ----------

        private void TryInitClient()
        {
            if (service != null)
                return;

            if (target == null)
                ResolveTarget();

            if (target == null)
                return;

            service = BuffService.CreateViewOnly();

            if (debugMode)
                Debug.Log("[BuffSystem] Client view-only service initialized", this);
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

            TryInitServer();
            if (service == null)
                return null;

            return service.AddBuff(cfg, target, source, lifetimeMode);
        }

        public void Remove(BuffInstance inst)
        {
            if (!IsServerStarted || service == null || inst == null)
                return;

            service.RemoveBuff(inst);
        }

        public void RemoveBySource(IBuffSource source)
        {
            if (!IsServerStarted || service == null || source == null)
                return;

            service.RemoveBySource(source);
        }

        public void ClearAll()
        {
            if (!IsServerStarted || service == null)
                return;

            service.ClearAll();
        }

        // =====================================================
        // SERVER EVENTS
        // =====================================================

        private void HandleBuffAdded(BuffInstance inst)
        {
            OnBuffAdded?.Invoke(inst);
            SyncActiveBuffs();
        }

        private void HandleBuffRemoved(BuffInstance inst)
        {
            OnBuffRemoved?.Invoke(inst);
            SyncActiveBuffs();
        }

        private void SyncActiveBuffs()
        {
            if (!IsServerStarted || service == null)
                return;

            var newIds = new HashSet<string>();

            foreach (var buff in service.Active)
            {
                if (buff?.Config != null)
                    newIds.Add(buff.Config.buffId);
            }

            // remove obsolete
            for (int i = ActiveBuffIds.Count - 1; i >= 0; i--)
            {
                if (!newIds.Contains(ActiveBuffIds[i]))
                    ActiveBuffIds.RemoveAt(i);
            }

            // add new
            foreach (var id in newIds)
            {
                if (!ActiveBuffIds.Contains(id))
                    ActiveBuffIds.Add(id);
            }
        }

        // =====================================================
        // CLIENT SYNC (VIEW ONLY)
        // =====================================================

        private void OnBuffIdsChanged(
            SyncListOperation op,
            int index,
            string oldItem,
            string newItem,
            bool asServer)
        {
            if (asServer)
                return;

            SyncClientState();
        }

        public bool HasBuffFromSource(IBuffSource source)
        {
            if (service == null || source == null)
                return false;

            foreach (var buff in service.Active)
            {
                if (buff.Source == source)
                    return true;
            }

            return false;
        }


        private void SyncClientState()
        {
            if (service == null)
                return;

            var local = new List<BuffInstance>(service.Active);
            var serverIds = new HashSet<string>(ActiveBuffIds);

            // REMOVE
            foreach (var buff in local)
            {
                if (buff?.Config != null &&
                    !serverIds.Contains(buff.Config.buffId))
                {
                    service.RemoveBuff(buff);
                }
            }

            // ADD (view-only)
            foreach (var id in serverIds)
            {
                bool exists = false;

                foreach (var b in service.Active)
                {
                    if (b?.Config?.buffId == id)
                    {
                        exists = true;
                        break;
                    }
                }

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
