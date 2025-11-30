using UnityEngine;
using System.Collections.Generic;
using Features.Buffs.Domain;
using Features.Buffs.UnityIntegration;

namespace Features.Buffs.Application
{
    public class BuffSystem : MonoBehaviour
    {
        private IBuffTarget _target;
        private BuffExecutor _executor;
        private BuffService _service;

        public BuffService Service => _service;
        public IReadOnlyList<BuffInstance> Active => _service?.Active;

        public event System.Action<BuffInstance> OnBuffAdded;
        public event System.Action<BuffInstance> OnBuffRemoved;

        private void Awake()
        {
            _target =
                GetComponent<IBuffTarget>() ??
                GetComponentInChildren<IBuffTarget>() ??
                GetComponentInParent<IBuffTarget>();

            if (_target == null)
                Debug.LogError($"[BuffSystem] IBuffTarget not found on {name}");
        }

        private void Start()
        {
            TryInit();
        }

        private void TryInit()
        {
            if (_executor == null)
                _executor = FindFirstObjectByType<BuffExecutor>();

            if (_executor != null && _service == null)
            {
                _service = new BuffService(_executor);

                // ВАЖНО: перенаправляем события из сервиса наружу
                _service.OnAdded += inst => OnBuffAdded?.Invoke(inst);
                _service.OnRemoved += inst => OnBuffRemoved?.Invoke(inst);
            }
        }


        private void Update()
        {
            if (_executor == null || _service == null)
            {
                TryInit();
                return;
            }

            _service.Tick(Time.deltaTime);
        }

        // ------------------------------
        // PUBLIC API
        // ------------------------------

        public BuffInstance Add(BuffSO cfg)
        {
            if (cfg == null) return null;
            TryInit();

            var inst = _service.AddBuff(cfg, _target);
            if (inst != null)
                OnBuffAdded?.Invoke(inst);

            return inst;
        }

        public void Remove(BuffInstance inst)
        {
            TryInit();
            _service.RemoveBuff(inst);
            OnBuffRemoved?.Invoke(inst);
        }

        public BuffService GetServiceSafe()
        {
            TryInit();
            return _service;
        }
    }
}
