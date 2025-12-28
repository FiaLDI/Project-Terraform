using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Features.Buffs.Domain;
using Features.Stats.Domain;
using FishNet.Object;

namespace Features.Buffs.Application
{
    public class BuffSystem : NetworkBehaviour
    {
        private IBuffTarget _target;
        private BuffExecutor _executor;
        private BuffService _service;
        private IStatsFacade _stats;

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

        private void Start()
        {
            StartCoroutine(InitRoutine());
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
                Debug.LogWarning("[BuffSystem] Target not resolved yet", this);
                return;
            }

            if (_executor == null)
            {
                _executor = FindObjectOfType<BuffExecutor>(includeInactive: false);
                
                if (_executor == null)
                {
                    var executorGO = GameObject.Find("BuffExecutor");
                    if (executorGO != null)
                    {
                        _executor = executorGO.GetComponent<BuffExecutor>();
                    }
                }
                
                if (_executor == null)
                {
                    Debug.LogError("[BuffSystem] BuffExecutor not found! Make sure it exists in the scene and is active.", this);
                    return;
                }
                
                Debug.Log("[BuffSystem] Found BuffExecutor", this);
            }

            // Инициализируем сервис
            if (_service == null)
            {
                _service = new BuffService(_executor);
                
                _service.OnAdded += inst => {
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
                };

                _service.OnRemoved += inst => {
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
                };
                
                Debug.Log("[BuffSystem] BuffService initialized successfully", this);
            }
        }


        public BuffInstance Add(BuffSO cfg)
        {
            // 🟢 ИСПРАВЛЕНИЕ: Проверяем что сервис готов
            if (!ServiceReady)
            {
                Debug.LogWarning("[BuffSystem] Service not ready, trying to initialize...", this);
                TryInit();
            }
            
            if (!ServiceReady)
            {
                Debug.LogError("[BuffSystem] Service still not ready after init!", this);
                return null;
            }

            // 🟢 ИСПРАВЛЕНИЕ: Проверяем что cfg и target валидны
            if (cfg == null)
            {
                Debug.LogError("[BuffSystem] Cannot add null buff config!", this);
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
                
                // 🟢 ИСПРАВЛЕНИЕ: Синхронизируем по сети если сервер
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
            // 🟢 ИСПРАВЛЕНИЕ: Проверяем что instance валиден
            if (inst == null)
            {
                Debug.LogWarning("[BuffSystem] Cannot remove null buff instance", this);
                return;
            }

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
            Debug.Log($"[BuffSystem] RpcApplyBuffToAll: {buffId}", this);
            
            if (IsServer)
                return; // Сервер уже применил

            // На клиентах применить баф
            if (_target?.GameObject != null && ServiceReady)
            {
                var buffSO = FindBuffById(buffId);
                if (buffSO != null)
                {
                    try
                    {
                        _service.AddBuff(buffSO, _target);
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"[BuffSystem] Error in RPC ApplyBuff: {ex.Message}", this);
                    }
                }
            }
        }

        [ObserversRpc]
        private void RpcRemoveBuffFromAll(string buffId)
        {
            Debug.Log($"[BuffSystem] RpcRemoveBuffFromAll: {buffId}", this);
            
            // 🟢 ИСПРАВЛЕНИЕ: Проверяем что service и active не null
            if (_service?.Active == null)
                return;

            try
            {
                var toRemove = new System.Collections.Generic.List<BuffInstance>();
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
            Debug.Log($"[BuffSystem] Buff added (notification): {buffId}", this);
        }

        [ObserversRpc]
        private void RpcNotifyBuffRemoved(string buffId)
        {
            Debug.Log($"[BuffSystem] Buff removed (notification): {buffId}", this);
        }

        /* ================= HELPERS ================= */

        private BuffSO FindBuffById(string buffId)
        {
            // 🟢 ИСПРАВЛЕНИЕ: Загружаем из ресурсов с проверкой
            try
            {
                var allBuffs = UnityEngine.Resources.LoadAll<BuffSO>("Buffs");
                
                if (allBuffs == null || allBuffs.Length == 0)
                {
                    Debug.LogWarning("[BuffSystem] No buffs found in Resources/Buffs", this);
                    return null;
                }

                foreach (var buff in allBuffs)
                {
                    if (buff != null && buff.buffId == buffId)
                        return buff;
                }
                
                Debug.LogWarning($"[BuffSystem] Buff not found: {buffId}", this);
                return null;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[BuffSystem] Error finding buff: {ex.Message}", this);
                return null;
            }
        }
    }
}
