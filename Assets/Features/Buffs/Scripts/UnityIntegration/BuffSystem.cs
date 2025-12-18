using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Features.Buffs.Domain;
using Features.Stats.Domain;

namespace Features.Buffs.Application
{
    public class BuffSystem : MonoBehaviour
    {
        private IBuffTarget _target;
        private BuffExecutor _executor;
        private BuffService _service;
        private IStatsFacade _stats;

        public BuffService Service => _service;
        public IReadOnlyList<BuffInstance> Active => _service?.Active;
        public bool ServiceReady => _service != null;

        public IBuffTarget Target => _target; // ← ВОССТАНОВЛЕНО

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
                _service.Tick(Time.deltaTime);
        }

        private void TryResolveTarget()
        {
            _target =
                GetComponent<IBuffTarget>() ??
                GetComponentInChildren<IBuffTarget>() ??
                GetComponentInParent<IBuffTarget>();

            if (_target == null)
                Debug.LogError("[BuffSystem] No IBuffTarget found!", this);
        }

        private void TryInit()
        {
            if (_target == null) return;

            if (_executor == null)
                _executor = FindAnyObjectByType<BuffExecutor>();

            if (_executor == null) return;

            if (_service == null)
            {
                _service = new BuffService(_executor);

                _service.OnAdded += inst => OnBuffAdded?.Invoke(inst);
                _service.OnRemoved += inst => OnBuffRemoved?.Invoke(inst);
            }
        }

        public BuffInstance Add(BuffSO cfg)
        {
            if (!ServiceReady) return null;
            return _service.AddBuff(cfg, _target);
        }

        public void Remove(BuffInstance inst)
        {
            if (ServiceReady)
                _service.RemoveBuff(inst);
        }

        // ← ВОССТАНОВЛЕННАЯ ОБРАТНАЯ СОВМЕСТИМОСТЬ
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
    }
}
