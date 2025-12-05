using UnityEngine;
using System.Collections;
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

        public bool ServiceReady => _fullyInitialized && _service != null;

        public event System.Action<BuffInstance> OnBuffAdded;
        public event System.Action<BuffInstance> OnBuffRemoved;

        private bool _fullyInitialized = false;
        private bool _initStarted = false;

        public IBuffTarget Target => _target;


        // ------------------------------------------------------
        // LIFECYCLE
        // ------------------------------------------------------
        private void Awake()
        {
            TryResolveTarget();
        }

        private void Start()
        {
            if (!_initStarted)
                StartCoroutine(DelayedInit());
        }

        private IEnumerator DelayedInit()
        {
            _initStarted = true;

            // подождать 1 кадр, чтобы SystemsPrefab успел появиться
            yield return null;

            // ждём полного появления всех систем
            while (!_fullyInitialized)
            {
                TryInit();

                if (!_fullyInitialized)
                    yield return null;
            }
        }

        private void Update()
        {
            if (ServiceReady)
                _service.Tick(Time.deltaTime);
        }

        // ------------------------------------------------------
        // INIT LOGIC
        // ------------------------------------------------------

        private void TryResolveTarget()
        {
            if (_target != null) return;

            _target =
                GetComponent<IBuffTarget>() ??
                GetComponentInChildren<IBuffTarget>() ??
                GetComponentInParent<IBuffTarget>();

            if (_target == null)
                Debug.LogError($"[BuffSystem] No IBuffTarget found on {name}");
        }

        private void TryInit()
        {
            if (_fullyInitialized)
                return;

            TryResolveTarget();
            if (_target == null) return;

            if (_executor == null)
                _executor = FindFirstObjectByType<BuffExecutor>();

            if (_executor == null)
                return; // ждем SystemsPrefab

            if (_service == null)
            {
                _service = new BuffService(_executor);

                _service.OnAdded += x => OnBuffAdded?.Invoke(x);
                _service.OnRemoved += x => OnBuffRemoved?.Invoke(x);
            }

            _fullyInitialized = true;
        }

        // ------------------------------------------------------
        // PUBLIC API
        // ------------------------------------------------------

        public BuffInstance Add(BuffSO cfg)
        {
            if (cfg == null)
                return null;

            TryInit();
            if (!ServiceReady)
                return null;

            return _service.AddBuff(cfg, _target);
        }

        public void Remove(BuffInstance inst)
        {
            if (inst == null)
                return;

            TryInit();
            if (!ServiceReady)
                return;

            _service.RemoveBuff(inst);
        }

        public BuffService GetServiceSafe()
        {
            TryInit();
            return ServiceReady ? _service : null;
        }

        /// <summary>
        /// Позволяет внешним системам (BuffHUD, AbilityHUD) без ошибок ждать
        /// появления Service и Executor.
        /// </summary>
        public bool EnsureInit()
        {
            TryInit();
            return ServiceReady;
        }
    }
}
