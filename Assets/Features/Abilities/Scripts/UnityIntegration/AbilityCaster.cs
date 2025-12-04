using System;
using System.Collections;
using UnityEngine;
using Features.Abilities.Domain;
using Features.Abilities.UnityIntegration;
using Features.Abilities.Application;
using Features.Stats.Adapter;
using Features.Stats.Domain;

namespace Features.Abilities.Application
{
    [DefaultExecutionOrder(-150)]
    public class AbilityCaster : MonoBehaviour
    {
        [Header("Ability slots")]
        public AbilitySO[] abilities = new AbilitySO[5];

        [Header("Auto refs")]
        public Camera aimCamera;
        public LayerMask groundMask;
        public AbilityExecutor executor;

        private IEnergyStats _energy;
        public IEnergyStats Energy => _energy;

        private AbilityService _service;

        public event Action<AbilitySO> OnAbilityCast;
        public event Action<AbilitySO, float, float> OnCooldownChanged;
        public event Action<AbilitySO> OnChannelStarted;
        public event Action<AbilitySO, float, float> OnChannelProgress;
        public event Action<AbilitySO> OnChannelCompleted;
        public event Action<AbilitySO> OnChannelInterrupted;
        public event Action OnAbilitiesChanged;

        private void Awake()
        {
            AutoDetectCamera();
            StartCoroutine(DelayedInit());
        }

        private IEnumerator DelayedInit()
        {
            // ждём, пока StatsFacadeAdapter успеет инициализироваться
            yield return null;

            var statsAdapter = GetComponent<StatsFacadeAdapter>();
            if (statsAdapter != null)
                _energy = statsAdapter.Stats.Energy;

            if (_energy == null)
                Debug.LogError("[AbilityCaster] No IEnergyStats found!");

            FinalInit();
        }

        private void FinalInit()
        {
            AutoDetectCamera();

            if (executor == null)
                executor = AbilityExecutor.I;

            _service = new AbilityService(
                owner: gameObject,
                aimCamera: aimCamera,
                energy: _energy,
                groundMask: groundMask,
                executor: executor
            );

            _service.OnAbilityCast += a => OnAbilityCast?.Invoke(a);
            _service.OnCooldownChanged += (a, r, m) => OnCooldownChanged?.Invoke(a, r, m);
            _service.OnChannelStarted += a => OnChannelStarted?.Invoke(a);
            _service.OnChannelProgress += (a, t, m) => OnChannelProgress?.Invoke(a, t, m);
            _service.OnChannelCompleted += a => OnChannelCompleted?.Invoke(a);
            _service.OnChannelInterrupted += a => OnChannelInterrupted?.Invoke(a);
        }

        private void LateUpdate()
        {
            if (_service == null) return;

            if (executor == null && AbilityExecutor.I != null)
            {
                executor = AbilityExecutor.I;
                _service.SetExecutor(executor);
            }

            _service.Tick(Time.deltaTime);
        }

        private void AutoDetectCamera()
        {
            if (aimCamera != null) return;

            if (CameraRegistry.I?.CurrentCamera != null)
            {
                aimCamera = CameraRegistry.I.CurrentCamera;
                return;
            }

            var cross = FindAnyObjectByType<CrosshairController>();
            if (cross?.cam != null)
            {
                aimCamera = cross.cam;
                return;
            }

            aimCamera = Camera.main;
        }

        // ========================= PUBLIC API =========================

        public void SetAbilities(AbilitySO[] newAbilities)
        {
            for (int i = 0; i < abilities.Length; i++)
                abilities[i] = (newAbilities != null && i < newAbilities.Length)
                    ? newAbilities[i]
                    : null;

            OnAbilitiesChanged?.Invoke();
        }

        public void TryCast(int index)
        {
            if (index < 0 || index >= abilities.Length) return;

            var ab = abilities[index];
            if (ab == null) return;

            _service.TryCast(ab, index);
        }

        public float GetCooldown(int index)
        {
            if (index < 0 || index >= abilities.Length) return 0f;
            return _service.GetCooldownRemaining(abilities[index]);
        }

        /// <summary>
        /// Показывает КОРРЕКТНУЮ стоимость с учётом бафов (для Tooltip)
        /// </summary>
        public float GetFinalEnergyCost(AbilitySO ability)
        {
            if (ability == null) return 0f;

            float baseCost = ability.energyCost;
            if (_energy == null) return baseCost;

            return baseCost * _energy.CostMultiplier;
        }

        public bool IsChanneling => _service?.IsChanneling ?? false;
        public AbilitySO CurrentChannelAbility => _service?.CurrentChannelAbility;
    }
}
