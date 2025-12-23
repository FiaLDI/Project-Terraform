using System;
using System.Collections.Generic;
using Features.Abilities.Domain;
using Features.Abilities.UnityIntegration;
using Features.Stats.Domain;
using Features.Stats.UnityIntegration;
using UnityEngine;

namespace Features.Abilities.Application
{
    [DefaultExecutionOrder(-150)]
    public class AbilityCaster : MonoBehaviour
    {
        [Header("Ability slots")]
        [SerializeField] private AbilitySO[] abilities = new AbilitySO[5];
        public IReadOnlyList<AbilitySO> Abilities => abilities;

        [Header("Auto refs")]
        public LayerMask groundMask;
        public AbilityExecutor executor;

        private IEnergyStats energy;
        private AbilityService service;

        public bool IsReady { get; private set; }
        public IEnergyStats Energy => energy;

        /* ================= EVENTS ================= */

        public event Action OnAbilitiesChanged;
        public event Action<AbilitySO> OnAbilityCast;
        public event Action<AbilitySO, float, float> OnCooldownChanged;
        public event Action<AbilitySO> OnChannelStarted;
        public event Action<AbilitySO, float, float> OnChannelProgress;
        public event Action<AbilitySO> OnChannelCompleted;
        public event Action<AbilitySO> OnChannelInterrupted;

        /* ================= LIFECYCLE ================= */

        private void OnEnable()
        {
            PlayerStats.OnStatsReady += HandleStatsReady;
        }

        private void OnDisable()
        {
            PlayerStats.OnStatsReady -= HandleStatsReady;
        }

        /* ================= INIT ================= */

        private void HandleStatsReady(PlayerStats ps)
        {
            if (IsReady)
                return;

            energy = ps.Facade?.Energy;
            if (energy == null)
            {
                Debug.LogError("[AbilityCaster] IEnergyStats not found", this);
                return;
            }

            executor ??= AbilityExecutor.I;

            service = new AbilityService(
                owner: gameObject,
                energy: energy,
                groundMask: groundMask,
                executor: executor
            );

            service.OnAbilityCast += a => OnAbilityCast?.Invoke(a);
            service.OnCooldownChanged += (a, r, m) => OnCooldownChanged?.Invoke(a, r, m);
            service.OnChannelStarted += a => OnChannelStarted?.Invoke(a);
            service.OnChannelProgress += (a, t, m) => OnChannelProgress?.Invoke(a, t, m);
            service.OnChannelCompleted += a => OnChannelCompleted?.Invoke(a);
            service.OnChannelInterrupted += a => OnChannelInterrupted?.Invoke(a);

            IsReady = true;
        }

        private void LateUpdate()
        {
            if (!IsReady || service == null)
                return;

            if (executor == null && AbilityExecutor.I != null)
            {
                executor = AbilityExecutor.I;
                service.SetExecutor(executor);
            }

            service.Tick(Time.deltaTime);
        }

        /* ================= PUBLIC API ================= */

        public void SetAbilities(AbilitySO[] newAbilities)
        {
            Debug.Log($"[AbilityCaster] SetAbilities CALLED | this={name} id={GetInstanceID()} newLen={(newAbilities == null ? -1 : newAbilities.Length)}");

            for (int i = 0; i < abilities.Length; i++)
            {
                abilities[i] = (newAbilities != null && i < newAbilities.Length)
                    ? newAbilities[i]
                    : null;
                Debug.Log($"[AbilityCaster]  ability[{i}]={(newAbilities[i] ? newAbilities[i].name : "null")} icon={(newAbilities[i] && newAbilities[i].icon ? newAbilities[i].icon.name : "null")}");
            }


            Debug.Log($"[AbilityCaster] Invoking OnAbilitiesChanged | subs={(OnAbilitiesChanged == null ? 0 : OnAbilitiesChanged.GetInvocationList().Length)}");
            OnAbilitiesChanged?.Invoke();
        }

        public void TryCast(int index)
        {
            if (!IsReady || index < 0 || index >= abilities.Length)
                return;

            var ab = abilities[index];
            if (ab == null)
                return;

            service.TryCast(ab, index);
        }

        public float GetCooldown(int index)
        {
            if (!IsReady || index < 0 || index >= abilities.Length)
                return 0f;

            return service.GetCooldownRemaining(abilities[index]);
        }

        public bool IsChanneling => service?.IsChanneling ?? false;
        public AbilitySO CurrentChannelAbility => service?.CurrentChannelAbility;
    }
}
