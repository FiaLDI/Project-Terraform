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

        [Header("Library")]
        [SerializeField] private AbilityLibrarySO abilityLibrary;

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

            service.OnAbilityCast       += a => OnAbilityCast?.Invoke(a);
            service.OnCooldownChanged   += (a, r, m) => OnCooldownChanged?.Invoke(a, r, m);
            service.OnChannelStarted    += a => OnChannelStarted?.Invoke(a);
            service.OnChannelProgress   += (a, t, m) => OnChannelProgress?.Invoke(a, t, m);
            service.OnChannelCompleted  += a => OnChannelCompleted?.Invoke(a);
            service.OnChannelInterrupted+= a => OnChannelInterrupted?.Invoke(a);

            if (abilityLibrary == null)
            {
                abilityLibrary = UnityEngine.Resources.Load<AbilityLibrarySO>(
                    "Databases/AbilityLibrary");
                if (abilityLibrary == null)
                    Debug.LogError("[AbilityCaster] AbilityLibrary not found in Resources/Databases/AbilityLibrary", this);
            }

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
            for (int i = 0; i < abilities.Length; i++)
                abilities[i] = (newAbilities != null && i < newAbilities.Length)
                    ? newAbilities[i]
                    : null;

            OnAbilitiesChanged?.Invoke();
        }

        /// <summary>
        /// Серверный вызов: пытается кастануть и, если успех, возвращает AbilityContext.
        /// </summary>
        public bool TryCastWithContext(int index, out AbilitySO ability, out AbilityContext ctx)
        {
            ability = null;
            ctx     = default;

            if (!IsReady || index < 0 || index >= abilities.Length)
                return false;

            ability = abilities[index];
            if (ability == null)
                return false;

            bool ok = service.TryCast(ability, index);
            if (!ok)
                return false;

            // контекст берём из сервиса
            ctx = ability.castType == AbilityCastType.Instant
                ? service.LastInstantContext
                : service.LastChannelContext;

            return true;
        }

        /// <summary>
        /// Клиентский вызов из ObserversRpc: проиграть уже подтверждённый каст.
        /// </summary>
        // в AbilityCaster.PlayRemoteCast
        public void PlayRemoteCast(AbilitySO ability, int slot, AbilityContext ctx)
        {
            if (!IsReady || ability == null)
                return;

            // перезаписываем owner локальным объектом игрока
            ctx = new AbilityContext(
                owner: gameObject,
                targetPoint: ctx.TargetPoint,
                direction: ctx.Direction,
                slotIndex: slot,
                yaw: ctx.Yaw,
                pitch: ctx.Pitch
            );

            OnAbilityCast?.Invoke(ability);
            executor.Execute(ability, ctx);
        }


        public float GetCooldown(int index)
        {
            if (!IsReady || index < 0 || index >= abilities.Length)
                return 0f;

            return service.GetCooldownRemaining(abilities[index]);
        }

        public AbilitySO FindAbilityById(string id)
        {
            if (abilityLibrary == null || string.IsNullOrEmpty(id))
                return null;

            return abilityLibrary.FindById(id);
        }

        public bool IsChanneling => service?.IsChanneling ?? false;
        public AbilitySO CurrentChannelAbility => service?.CurrentChannelAbility;
    }
}
