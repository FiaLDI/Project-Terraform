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

            TryInit();
            BuffTickSystem.Register(this);
        }

        public override void OnStopServer()
        {
            base.OnStopServer();

            BuffTickSystem.Unregister(this);
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            ActiveBuffIds.OnChange += OnBuffIdsChanged;
            TryInit(); // client view-only service
        }

        public override void OnStopClient()
        {
            base.OnStopClient();

            ActiveBuffIds.OnChange -= OnBuffIdsChanged;
        }

        // =====================================================
        // TICK (CALLED ONLY BY BuffTickSystem)
        // =====================================================

        public void Tick(float dt)
        {
            if (!IsServerStarted || !ServiceReady)
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

        private void TryInit()
        {
            if (target == null)
                ResolveTarget();

            if (target == null)
                return;

            if (executor == null)
            {
                executor = BuffExecutor.Instance ?? FindObjectOfType<BuffExecutor>();
                if (executor == null)
                    return;
            }

            if (service != null)
                return;

            service = new BuffService(executor);
            service.OnAdded += HandleBuffAdded;
            service.OnRemoved += HandleBuffRemoved;

            if (debugMode)
                Debug.Log("[BuffSystem] Service initialized", this);
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

            TryInit();
            if (!ServiceReady)
                return null;

            return service.AddBuff(cfg, target, source, lifetimeMode);
        }

        public void Remove(BuffInstance inst)
        {
            if (!IsServerStarted || !ServiceReady || inst == null)
                return;

            service.RemoveBuff(inst);
        }

        public void RemoveBySource(IBuffSource source)
        {
            if (!IsServerStarted || !ServiceReady || source == null)
                return;

            service.RemoveBySource(source);
        }

        public void ClearAll()
        {
            if (!IsServerStarted || !ServiceReady)
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
            if (!IsServerStarted || service?.Active == null)
                return;

            ActiveBuffIds.Clear();

            foreach (var buff in service.Active)
            {
                if (buff?.Config != null)
                    ActiveBuffIds.Add(buff.Config.buffId);
            }

            if (debugMode)
                Debug.Log($"[BuffSystem] Synced {ActiveBuffIds.Count} buffs", this);
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

        private void SyncClientState()
        {
            if (!ServiceReady)
                return;

            var local = new List<BuffInstance>(service.Active);
            var serverIds = new HashSet<string>(ActiveBuffIds);

            // REMOVE
            foreach (var buff in local)
            {
                if (buff?.Config != null && !serverIds.Contains(buff.Config.buffId))
                    service.RemoveBuff(buff);
            }

            // ADD (view only, no source)
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
