using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Features.Buffs.Domain;
using Features.Stats.Domain;
using FishNet.Object;
using Features.Buffs.Data;

namespace Features.Buffs.Application
{
    public class BuffSystem : NetworkBehaviour
    {
        [Header("Debug")]
        [SerializeField] private bool _debugMode;

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
            StartCoroutine(InitRoutine());
        }

        private IEnumerator InitRoutine()
        {
            // Ждем 1 кадр, чтобы точно все синглтоны проснулись
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

            // 1. Находим Executor (теперь через Singleton, это надежно)
            if (_executor == null)
            {
                _executor = BuffExecutor.Instance;

                // Если вдруг Instance еще null (очень странно, но бывает), пробуем найти
                if (_executor == null)
                {
                    _executor = FindObjectOfType<BuffExecutor>();
                }
                
                if (_executor == null)
                {
                    // Не кричим Error, а ждем следующей попытки (InitRoutine или Add вызовет повторно)
                    if (_debugMode) Debug.LogWarning("[BuffSystem] BuffExecutor not found yet. Waiting...", this);
                    return;
                }
            }

            // 2. Инициализируем сервис
            if (_service == null)
            {
                _service = new BuffService(_executor);
                
                // Подписки на события сервиса
                _service.OnAdded += HandleBuffAdded;
                _service.OnRemoved += HandleBuffRemoved;
                
                if (_debugMode) Debug.Log("[BuffSystem] BuffService initialized successfully", this);

                // 3. Применяем отложенные баффы
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
                    Add(buffCfg); // Рекурсивно вызовет Add, но теперь ServiceReady = true
                }
                _pendingBuffs.Clear();
            }
        }

        // Обработчики событий (вынесены для чистоты)
        private void HandleBuffAdded(BuffInstance inst)
        {
            try
            {
                OnBuffAdded?.Invoke(inst);
                if (IsServer)
                {
                    RpcNotifyBuffAdded(inst.Config.buffId);
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
                if (IsServer)
                {
                    RpcNotifyBuffRemoved(inst.Config.buffId);
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

            // Если сервис еще не готов — пробуем инициализировать
            if (!ServiceReady)
            {
                TryInit();
            }
            
            // Если все еще не готов (например, нет Executor в сцене)
            if (!ServiceReady)
            {
                if (_debugMode) Debug.LogWarning($"[BuffSystem] Service not ready. Queueing buff {cfg.buffId}", this);
                _pendingBuffs.Add(cfg);
                return null; // Вернем null, но бафф применится позже
            }

            if (_target == null)
            {
                Debug.LogError("[BuffSystem] Target is null, cannot add buff!", this);
                return null;
            }

            try
            {
                var buff = _service.AddBuff(cfg, _target);
                
                if (IsServer && buff != null)
                {
                    RpcApplyBuffToAll(cfg.buffId);
                }
                
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
                    
                    if (IsServer)
                    {
                        RpcRemoveBuffFromAll(inst.Config.buffId);
                    }
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

        /* ================= RPC ================= */

        [ObserversRpc]
        private void RpcApplyBuffToAll(string buffId)
        {
            if (IsServer) return; // Сервер уже применил

            // Если система еще не готова на клиенте — пробуем инициализировать
            if (!ServiceReady) TryInit();

            if (_target?.GameObject != null && ServiceReady)
            {
                var buffSO = FindBuffById(buffId);
                if (buffSO != null)
                {
                    try
                    {
                        // На клиенте просто добавляем в локальный сервис
                        // Executor сам применит эффекты
                        _service.AddBuff(buffSO, _target);
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"[BuffSystem] Error in RPC ApplyBuff: {ex.Message}", this);
                    }
                }
            }
            else
            {
                 // Если все еще не готовы — можно добавить в pending, но для RPC это сложнее (нужен конфиг)
                 // Обычно клиент успевает инициализироваться раньше, чем прилетит первый бафф
            }
        }

        [ObserversRpc]
        private void RpcRemoveBuffFromAll(string buffId)
        {
            if (_service?.Active == null) return;

            try
            {
                var toRemove = new List<BuffInstance>();
                foreach (var buff in _service.Active)
                {
                    if (buff?.Config != null && buff.Config.buffId == buffId)
                        toRemove.Add(buff);
                }

                foreach (var buff in toRemove)
                {
                    _service.RemoveBuff(buff);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[BuffSystem] Error in RPC RemoveBuff: {ex.Message}", this);
            }
        }

        [ObserversRpc]
        private void RpcNotifyBuffAdded(string buffId)
        {
            if (_debugMode) Debug.Log($"[BuffSystem] Buff added (notification): {buffId}", this);
        }

        [ObserversRpc]
        private void RpcNotifyBuffRemoved(string buffId)
        {
            if (_debugMode) Debug.Log($"[BuffSystem] Buff removed (notification): {buffId}", this);
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
