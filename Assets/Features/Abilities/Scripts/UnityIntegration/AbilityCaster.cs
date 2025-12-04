using System;
using UnityEngine;
using System.Collections;
using Features.Abilities.Domain;
using Features.Abilities.UnityIntegration;
using Features.Energy.Application;
using Features.Energy.Domain;
using Features.Stats.Adapter;

namespace Features.Abilities.Application
{
    public class AbilityCaster : MonoBehaviour
    {
        [Header("Ability slots")]
        public AbilitySO[] abilities = new AbilitySO[5];

        [Header("Auto refs")]
        public Camera aimCamera;
        public LayerMask groundMask;
        public AbilityExecutor executor;

        private IEnergy energy;
        public IEnergy Energy => energy;

        private AbilityService service;
        private EnergyCostService costService;

        public event Action<AbilitySO> OnAbilityCast;
        public event Action<AbilitySO, float, float> OnCooldownChanged;
        public event Action<AbilitySO> OnChannelStarted;
        public event Action<AbilitySO, float, float> OnChannelProgress;
        public event Action<AbilitySO> OnChannelCompleted;
        public event Action<AbilitySO> OnChannelInterrupted;
        public event Action OnAbilitiesChanged;


        // ================================================================
        // INIT
        // ================================================================
        private void Awake()
        {
            AutoDetectCamera();
            StartCoroutine(DelayedInit());
        }

        private IEnumerator DelayedInit()
        {
            // Даем StatsFacadeAdapter время инициализировать все адаптеры
            yield return null;

            var statsAdapter = GetComponent<StatsFacadeAdapter>();
            if (statsAdapter != null)
                energy = statsAdapter.EnergyStats;

            if (energy == null)
                Debug.LogError("[AbilityCaster] No EnergyStatsAdapter found!");

            FinalInit();
        }


        private void FinalInit()
        {
            AutoDetectCamera();
            executor = AbilityExecutor.I;

            // ----------------------------------------------------
            // Создаём сервис расчёта стоимости энергии
            // ----------------------------------------------------
            costService = new EnergyCostService();

            // локальные модификаторы
            foreach (var m in GetComponentsInChildren<IEnergyCostModifier>())
                costService.AddModifier(m);

            // глобальный поставщик
            if (GlobalEnergyCostProvider.I != null)
                costService.AddModifier(GlobalEnergyCostProvider.I);

            // ----------------------------------------------------
            // Создаём AbilityService (главный управляющий)
            // ----------------------------------------------------
            service = new AbilityService(
                owner: gameObject,
                aimCamera: aimCamera,
                energy: energy,
                groundMask: groundMask,
                executor: executor,
                costService: costService
            );

            // EVENTS
            service.OnAbilityCast += a => OnAbilityCast?.Invoke(a);
            service.OnCooldownChanged += (a, r, m) => OnCooldownChanged?.Invoke(a, r, m);
            service.OnChannelStarted += a => OnChannelStarted?.Invoke(a);
            service.OnChannelProgress += (a, t, m) => OnChannelProgress?.Invoke(a, t, m);
            service.OnChannelCompleted += a => OnChannelCompleted?.Invoke(a);
            service.OnChannelInterrupted += a => OnChannelInterrupted?.Invoke(a);
        }


        // ================================================================
        // UPDATE
        // ================================================================
        private void LateUpdate()
        {
            if (service == null)
                return;

            // Late binding executor
            if (executor == null && AbilityExecutor.I != null)
            {
                executor = AbilityExecutor.I;
                service.SetExecutor(executor);
            }

            service.Tick(Time.deltaTime);
        }


        // ================================================================
        // HELPERS
        // ================================================================
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


        // ================================================================
        // PUBLIC API
        // ================================================================
        public void SetAbilities(AbilitySO[] newAbilities)
        {
            for (int i = 0; i < abilities.Length; i++)
                abilities[i] = (newAbilities != null && i < newAbilities.Length) ? newAbilities[i] : null;

            OnAbilitiesChanged?.Invoke();
        }

        public void TryCast(int index)
        {
            if (index < 0 || index >= abilities.Length) return;

            var ab = abilities[index];
            if (ab == null) return;

            service.TryCast(ab, index);
        }

        public float GetCooldown(int index)
        {
            if (index < 0 || index >= abilities.Length) return 0;
            return service.GetCooldownRemaining(abilities[index]);
        }

        public float GetFinalEnergyCost(AbilitySO ability)
        {
            if (ability == null) return 0f;
            if (costService == null) return ability.energyCost;

            return costService.ApplyModifiers(ability.energyCost);
        }

        public bool IsChanneling => service?.IsChanneling ?? false;
        public AbilitySO CurrentChannelAbility => service?.CurrentChannelAbility;
    }
}
