using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Features.Buffs.Domain;
using Features.Stats.Domain;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Features.Buffs.Data;


namespace Features.Buffs.Application
{
    public class BuffSystem : NetworkBehaviour
    {
        [Header("Debug")]
        [SerializeField] private bool _debugMode;

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // СИНХРОНИЗАЦИЯ: SyncList для ID активных баффов
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        public readonly SyncList<string> ActiveBuffIds = new SyncList<string>();

        private IBuffTarget _target;
        private BuffExecutor _executor;
        private BuffService _service;
        private IStatsFacade _stats;

        // Очередь бафов, которые попытались добавить ДО инициализации сервиса
        private readonly List<BuffSO> _pendingBuffs = new List<BuffSO>();

        public BuffService Service => _service;
        public IReadOnlyList<BuffInstance> Active => _service?.Active;
        public bool ServiceReady => _service != null;
        public IBuffTarget Target => _target;

        public event System.Action<BuffInstance> OnBuffAdded;
        public event System.Action<BuffInstance> OnBuffRemoved;

        private void Awake()
        {
            TryResolveTarget();
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            ActiveBuffIds.OnChange += OnBuffIdsChanged;
            StartCoroutine(InitRoutine());
        }

        public override void OnStopClient()
        {
            base.OnStopClient();
            ActiveBuffIds.OnChange -= OnBuffIdsChanged;
        }

        private IEnumerator InitRoutine()
        {
            yield return null;
            TryInit();
        }

        private void Update()
        {
            if (ServiceReady)
            {
                _service.Tick(Time.deltaTime);
            }
        }

        private void TryResolveTarget()
        {
            _target =
                GetComponent<IBuffTarget>() ??
                GetComponentInChildren<IBuffTarget>() ??
                GetComponentInParent<IBuffTarget>();

            if (_target == null)
            {
                Debug.LogError("[BuffSystem] No IBuffTarget found on this GameObject!", this);
            }
        }

        private void TryInit()
        {
            if (_target == null)
            {
                if (_debugMode) Debug.LogWarning("[BuffSystem] Target not resolved yet", this);
                return;
            }

            if (_executor == null)
            {
                _executor = BuffExecutor.Instance;

                if (_executor == null)
                {
                    _executor = FindObjectOfType<BuffExecutor>();
                }

                if (_executor == null)
                {
                    if (_debugMode) Debug.LogWarning("[BuffSystem] BuffExecutor not found yet. Waiting...", this);
                    return;
                }
            }

            if (_service == null)
            {
                _service = new BuffService(_executor);
                _service.OnAdded += HandleBuffAdded;
                _service.OnRemoved += HandleBuffRemoved;

                if (_debugMode) Debug.Log("[BuffSystem] BuffService initialized successfully", this);
                ProcessPendingBuffs();
            }
        }

        private void ProcessPendingBuffs()
        {
            if (_pendingBuffs.Count > 0)
            {
                if (_debugMode) Debug.Log($"[BuffSystem] Processing {_pendingBuffs.Count} pending buffs...", this);
                foreach (var buffCfg in _pendingBuffs)
                {
                    Add(buffCfg);
                }
                _pendingBuffs.Clear();
            }
        }

        private void HandleBuffAdded(BuffInstance inst)
        {
            try
            {
                OnBuffAdded?.Invoke(inst);

                if (IsServerInitialized)
                {
                    UpdateActivBuffsSync();
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[BuffSystem] Error in OnBuffAdded event: {ex.Message}", this);
            }
        }

        private void HandleBuffRemoved(BuffInstance inst)
        {
            try
            {
                OnBuffRemoved?.Invoke(inst);

                if (IsServerInitialized)
                {
                    UpdateActivBuffsSync();
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[BuffSystem] Error in OnBuffRemoved event: {ex.Message}", this);
            }
        }

        public BuffInstance Add(BuffSO cfg)
        {
            if (cfg == null)
            {
                Debug.LogError("[BuffSystem] Cannot add null buff config!", this);
                return null;
            }

            if (!ServiceReady)
            {
                TryInit();
            }

            if (!ServiceReady)
            {
                Debug.LogWarning($"[BuffSystem] Service not ready. Queueing buff {cfg.buffId}", this);
                _pendingBuffs.Add(cfg);
                return null;
            }

            if (_target == null)
            {
                Debug.LogError("[BuffSystem] Target is null, cannot add buff!", this);
                return null;
            }

            try
            {
                var buff = _service.AddBuff(cfg, _target);
                return buff;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[BuffSystem] Error adding buff: {ex.Message}", this);
                return null;
            }
        }

        public void Remove(BuffInstance inst)
        {
            if (inst == null) return;

            if (ServiceReady)
            {
                try
                {
                    _service.RemoveBuff(inst);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[BuffSystem] Error removing buff: {ex.Message}", this);
                }
            }
        }

        public BuffService GetServiceSafe()
        {
            return _service;
        }

        public void SetTarget(IBuffTarget target)
        {
            _target = target;
        }

        public void SetStats(IStatsFacade stats)
        {
            _stats = stats;
        }

        /* ================= СИНХРОНИЗАЦИЯ ЧЕРЕЗ SYNCLIST ================= */

        /// <summary>
        /// Обновляет SyncList состояния активных баффов на сервере.
        /// Автоматически отправляет на клиентов через FishNet.
        /// </summary>
        private void UpdateActivBuffsSync()
        {
            if (!IsServerInitialized || _service?.Active == null) return;

            var currentIds = new HashSet<string>();
            foreach (var buff in _service.Active)
            {
                if (buff?.Config != null)
                {
                    currentIds.Add(buff.Config.buffId);
                }
            }

            ActiveBuffIds.Clear();
            foreach (var id in currentIds)
            {
                ActiveBuffIds.Add(id);
            }

            if (_debugMode)
                Debug.Log($"[BuffSystem] Synced {ActiveBuffIds.Count} active buffs to clients", this);
        }

        /// <summary>
        /// Вызывается на клиентах когда SyncList изменяется.
        /// </summary>
        private void OnBuffIdsChanged(SyncListOperation op, int index, string oldItem, string newItem, bool asServer)
        {
            if (asServer) return;

            if (_debugMode)
                Debug.Log($"[BuffSystem] Buff state changed on client. Op={op}", this);

            // Откладываем синхронизацию до конца кадра
            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(SyncNextFrame());
            }
        }

        private IEnumerator SyncNextFrame()
        {
            yield return new WaitForEndOfFrame();
            SyncBuffStateWithServer();
        }

        /// <summary>
        /// ПУЛЕНЕПРОБИВАЕМАЯ синхронизация без foreach/Enumerator
        /// Использует индексный доступ вместо foreach для избежания InvalidOperationException
        /// </summary>
        private void SyncBuffStateWithServer()
        {
            if (!ServiceReady) return;

            // 1. БЕЗОПАСНО копируем ЛОКАЛЬНЫЕ баффы через индекс (без foreach)
            var currentBuffs = new List<BuffInstance>();
            var sourceList = _service.Active;

            if (sourceList != null)
            {
                int count = sourceList.Count;
                for (int i = 0; i < count; i++)
                {
                    try
                    {
                        if (i < sourceList.Count)
                        {
                            var b = sourceList[i];
                            if (b != null) currentBuffs.Add(b);
                        }
                    }
                    catch { /* Игнорируем ошибки доступа при гонке */ }
                }
            }

            // 2. БЕЗОПАСНО копируем СЕРВЕРНЫЕ ID через индекс
            var serverIds = new List<string>();
            var syncList = ActiveBuffIds;

            if (syncList != null)
            {
                int count = syncList.Count;
                for (int i = 0; i < count; i++)
                {
                    try
                    {
                        if (i < syncList.Count)
                        {
                            serverIds.Add(syncList[i]);
                        }
                    }
                    catch { /* Игнорируем */ }
                }
            }

            // 3. Логика синхронизации (работаем только с локальными копиями)
            var clientBuffIds = new HashSet<string>();
            foreach (var buff in currentBuffs)
            {
                if (buff?.Config != null) clientBuffIds.Add(buff.Config.buffId);
            }

            // === УДАЛЕНИЕ (есть локально, нет на сервере) ===
            foreach (var buff in currentBuffs)
            {
                if (buff?.Config != null && !serverIds.Contains(buff.Config.buffId))
                {
                    _service.RemoveBuff(buff);
                    if (_debugMode) Debug.Log($"[BuffSystem] Removed buff {buff.Config.buffId} (not on server)", this);
                }
            }

            // === ДОБАВЛЕНИЕ (есть на сервере, нет локально) ===
            foreach (var serverId in serverIds)
            {
                if (!clientBuffIds.Contains(serverId))
                {
                    var buffSO = FindBuffById(serverId);
                    if (buffSO != null && _target != null)
                    {
                        _service.AddBuff(buffSO, _target);
                        if (_debugMode) Debug.Log($"[BuffSystem] Added buff {serverId} (from server)", this);
                    }
                }
            }
        }

        /* ================= HELPERS ================= */

        private BuffSO FindBuffById(string buffId)
        {
            try
            {
                var buff = BuffRegistrySO.Instance.GetById(buffId);

                if (buff == null)
                {
                    Debug.LogWarning($"[BuffSystem] Buff ID '{buffId}' not found in Registry!", this);
                }

                return buff;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[BuffSystem] Error finding buff: {ex.Message}", this);
                return null;
            }
        }
    }
}