using System;
using System.Collections.Generic;
using Features.Abilities.Domain;
using Features.Abilities.UnityIntegration;
using Features.Stats.Domain;
using Features.Stats.UnityIntegration;
using FishNet.Object;
using UnityEngine;

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

        [Header("Network Sync")]
        [SerializeField] private float cooldownSyncInterval = 0.5f;

        private float[] cooldownValues = new float[5];
        private float[] lastSyncedCooldowns = new float[5];
        private float syncTimer;

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
            for (int i = 0; i < cooldownValues.Length; i++)
            {
                cooldownValues[i] = 0f;
                lastSyncedCooldowns[i] = 0f;
            }
        }

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

            // ===== прокидываем события =====
            service.OnAbilityCast += a => OnAbilityCast?.Invoke(a);
            service.OnCooldownChanged += (a, r, m) => OnCooldownChanged?.Invoke(a, r, m);
            service.OnChannelStarted += a => OnChannelStarted?.Invoke(a);
            service.OnChannelProgress += (a, t, m) => OnChannelProgress?.Invoke(a, t, m);
            service.OnChannelCompleted += a => OnChannelCompleted?.Invoke(a);
            service.OnChannelInterrupted += a => OnChannelInterrupted?.Invoke(a);

            if (abilityLibrary == null)
            {
                abilityLibrary = UnityEngine.Resources.Load<AbilityLibrarySO>("Databases/AbilityLibrary");
                if (abilityLibrary == null)
                    Debug.LogError("[AbilityCaster] AbilityLibrary not found", this);
            }

            IsReady = true;
        }

        private void LateUpdate()
        {
            if (!IsReady || service == null)
                return;

            // 🔐 СТРОГО: логика способностей живёт только на сервере
            if (!IsServerInitialized)
                return;

            service.Tick(Time.deltaTime);

            syncTimer += Time.deltaTime;
            if (syncTimer >= cooldownSyncInterval)
            {
                syncTimer = 0f;
                SyncCooldownsIfChanged();
            }
        }

        /* ================= SYNC ================= */

        private void SyncCooldownsIfChanged()
        {
            for (int i = 0; i < abilities.Length; i++)
            {
                if (abilities[i] == null)
                    continue;

                float newCooldown = service.GetCooldownRemaining(abilities[i]);
                cooldownValues[i] = newCooldown;

                if (Mathf.Abs(lastSyncedCooldowns[i] - newCooldown) > 0.1f)
                {
                    lastSyncedCooldowns[i] = newCooldown;
                    RpcSyncCooldown(i, newCooldown);
                }
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
        /// ❗ Вызывается ТОЛЬКО сервером (через NetAdapter)
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

        public float GetCooldown(int index)
        {
            if (!IsReady || index < 0 || index >= abilities.Length)
                return 0f;

            return cooldownValues[index];
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
        private void RpcSyncCooldown(int slotIndex, float cooldownValue)
        {
            if (slotIndex < 0 || slotIndex >= cooldownValues.Length)
                return;

            cooldownValues[slotIndex] = cooldownValue;

            if (slotIndex < abilities.Length && abilities[slotIndex] != null)
            {
                OnCooldownChanged?.Invoke(
                    abilities[slotIndex],
                    cooldownValue,
                    abilities[slotIndex].cooldown
                );
            }
        }
    }
}
