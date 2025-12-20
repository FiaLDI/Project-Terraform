using System;
using System.Collections;
using UnityEngine;
using Features.Abilities.Domain;
using Features.Abilities.UnityIntegration;
using Features.Abilities.Application;
using Features.Stats.Adapter;
using Features.Stats.Domain;
using Features.Camera.UnityIntegration;

namespace Features.Abilities.Application
{
    /// <summary>
    /// UnityIntegration-адаптер для AbilityService.
    /// НЕ содержит логики способностей.
    /// НЕ работает с камерой напрямую.
    /// Только перенаправляет команды.
    /// </summary>
    [DefaultExecutionOrder(-150)]
    public class AbilityCaster : MonoBehaviour
    {
        [Header("Ability slots")]
        public AbilitySO[] abilities = new AbilitySO[5];

        [Header("Auto refs")]
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
            StartCoroutine(DelayedInit());
        }

        /// <summary>
        /// Ждём StatsFacadeAdapter, чтобы IEnergy стало доступно.
        /// </summary>
        private IEnumerator DelayedInit()
        {
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
            if (executor == null)
                executor = AbilityExecutor.I;

            _service = new AbilityService(
                owner: (object)gameObject, 
                energy: _energy,
                groundMask: groundMask,
                executor: executor
            );

            // Подписываем внутренние события AbilityService
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

            // executor может появиться позже
            if (executor == null && AbilityExecutor.I != null)
            {
                executor = AbilityExecutor.I;
                _service.SetExecutor(executor);
            }

            _service.Tick(Time.deltaTime);
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
