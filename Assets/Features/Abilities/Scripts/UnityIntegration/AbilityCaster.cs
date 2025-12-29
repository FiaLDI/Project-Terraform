using System;
using System.Collections.Generic;
using Features.Abilities.Domain;
using Features.Abilities.UnityIntegration;
using Features.Stats.Domain;
using Features.Stats.UnityIntegration;
using UnityEngine;
using FishNet.Object;


namespace Features.Abilities.Application
{
    [DefaultExecutionOrder(-150)]
    public class AbilityCaster : NetworkBehaviour
    {
        [Header("Ability slots")]
        [SerializeField] private AbilitySO[] abilities = new AbilitySO[5];
        public IReadOnlyList<AbilitySO> Abilities => abilities;

        [Header("Auto refs")]
        public LayerMask groundMask;
        public AbilityExecutor executor;

        [Header("Library")]
        [SerializeField] private AbilityLibrarySO abilityLibrary;

        // 🟢 ИСПРАВЛЕНИЕ: простые float массивы вместо NetworkVariable
        private float[] cooldownValues = new float[5];

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

        private void Awake()
        {
            // Инициализируем массив cooldown значений
            for (int i = 0; i < cooldownValues.Length; i++)
            {
                cooldownValues[i] = 0f;
            }
        }

        private void OnEnable()
        {
            Debug.Log($"[AbilityCaster] OnEnable() - {gameObject.name}", this);
            PlayerStats.OnStatsReady += HandleStatsReady;
        }

        private void OnDisable()
        {
            Debug.Log($"[AbilityCaster] OnDisable() - {gameObject.name}", this);
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

            service.OnAbilityCast += OnAbilityCastHandler;
            service.OnCooldownChanged += OnCooldownChangedHandler;
            service.OnChannelStarted += a => OnChannelStarted?.Invoke(a);
            service.OnChannelProgress += (a, t, m) => OnChannelProgress?.Invoke(a, t, m);
            service.OnChannelCompleted += a => OnChannelCompleted?.Invoke(a);
            service.OnChannelInterrupted += a => OnChannelInterrupted?.Invoke(a);

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

            // 🟢 ИСПРАВЛЕНИЕ: обновляем cooldowns локально и синхронизируем через RPC
            if (IsServer)
            {
                for (int i = 0; i < abilities.Length; i++)
                {
                    if (abilities[i] != null)
                    {
                        float newCooldown = service.GetCooldownRemaining(abilities[i]);

                        // Отправляем RPC если cooldown изменился на >0.01 (чтобы не спамить сетевые пакеты)
                        if (Mathf.Abs(cooldownValues[i] - newCooldown) > 0.01f)
                        {
                            cooldownValues[i] = newCooldown;
                            RpcSyncCooldown(i, newCooldown);
                        }
                    }
                }
            }
        }

        /* ================= HANDLERS ================= */

        private void OnAbilityCastHandler(AbilitySO ability)
        {
            OnAbilityCast?.Invoke(ability);

            // 🟢 ИСПРАВЛЕНИЕ: синхронизируем через RPC
            if (IsServer)
            {
                int slotIndex = System.Array.IndexOf(abilities, ability);
                if (slotIndex >= 0)
                {
                    RpcNotifyAbilityCast(slotIndex);
                }
            }
        }

        private void OnCooldownChangedHandler(AbilitySO ability, float remaining, float max)
        {
            OnCooldownChanged?.Invoke(ability, remaining, max);

            int slotIndex = System.Array.IndexOf(abilities, ability);
            if (IsServer && slotIndex >= 0)
            {
                cooldownValues[slotIndex] = remaining;
                RpcSyncCooldown(slotIndex, remaining);
            }
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
            ctx = default;

            if (!IsReady || index < 0 || index >= abilities.Length)
                return false;

            ability = abilities[index];
            if (ability == null)
                return false;

            bool ok = service.TryCast(ability, index);
            if (!ok)
                return false;

            ctx = ability.castType == AbilityCastType.Instant
                ? service.LastInstantContext
                : service.LastChannelContext;

            return true;
        }

        /// <summary>
        /// Клиентский вызов из ObserversRpc: проиграть уже подтверждённый каст.
        /// </summary>
        public void PlayRemoteCast(AbilitySO ability, int slot, AbilityContext ctx)
        {
            if (!IsReady || ability == null)
                return;

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

        /// <summary>
        /// 🟢 ИСПРАВЛЕНИЕ: получить cooldown синхронизированный через сеть
        /// </summary>
        public float GetCooldown(int index)
        {
            if (!IsReady || index < 0 || index >= abilities.Length)
                return 0f;

            // Используем локальное значение которое синхронизируется через RPC
            if (cooldownValues[index] > 0)
            {
                return cooldownValues[index];
            }

            // Fallback на локальный сервис
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

        /* ================= RPC ================= */

        [ObserversRpc]
        private void RpcNotifyAbilityCast(int slotIndex)
        {
            Debug.Log($"[AbilityCaster] Ability cast notification for slot {slotIndex}", this);
            if (slotIndex >= 0 && slotIndex < abilities.Length && abilities[slotIndex] != null)
            {
                OnAbilityCast?.Invoke(abilities[slotIndex]);
            }
        }

        /// <summary>
        /// 🟢 ИСПРАВЛЕНИЕ: синхронизируем cooldown значение через RPC
        /// </summary>
        [ObserversRpc]
        private void RpcSyncCooldown(int slotIndex, float cooldownValue)
        {
            if (slotIndex >= 0 && slotIndex < cooldownValues.Length)
            {
                cooldownValues[slotIndex] = cooldownValue;
            }
        }
    }
}
