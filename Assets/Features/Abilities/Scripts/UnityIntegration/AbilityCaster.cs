using System;
using System.Collections.Generic;
using Features.Abilities.Domain;
using Features.Abilities.UnityIntegration;
using Features.Stats.Domain;
using Features.Stats.UnityIntegration;
using FishNet.Managing.Server;
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
        [SerializeField] private float cooldownSyncInterval = 0.5f; // раз в 0.5 сек (4-8 игроков - оптимально)

        private float[] cooldownValues = new float[5];
        private float[] lastSyncedCooldowns = new float[5]; // Отслеживаем что уже отправили
        private float syncTimer = 0f;

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

            // 🎯 Теперь не передаём ServerManager в конструктор
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
                    Debug.LogError("[AbilityCaster] AbilityLibrary not found", this);
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

            // 🎯 ОПТИМИЗАЦИЯ: Синхронизируем раз в X секунд (0.5s для 4-8 игроков)
            if (IsServerInitialized)
            {
                syncTimer += Time.deltaTime;

                if (syncTimer >= cooldownSyncInterval)
                {
                    syncTimer = 0f;
                    SyncCooldownsIfChanged();
                }
            }
        }

        /* ================= SYNC LOGIC ================= */

        /// <summary>
        /// 🎯 Отправляем только изменившиеся CD (экономим трафик)
        /// </summary>
        private void SyncCooldownsIfChanged()
        {
            for (int i = 0; i < abilities.Length; i++)
            {
                if (abilities[i] == null)
                    continue;

                float newCooldown = service.GetCooldownRemaining(abilities[i]);
                cooldownValues[i] = newCooldown;

                // Отправляем только если CD значимо изменился (>0.1 сек разницы)
                // Это фильтрует мелкие изменения и экономит трафик
                if (Mathf.Abs(lastSyncedCooldowns[i] - newCooldown) > 0.1f)
                {
                    lastSyncedCooldowns[i] = newCooldown;
                    RpcSyncCooldown(i, newCooldown);
                }
            }
        }

        /* ================= HANDLERS ================= */

        private void OnAbilityCastHandler(AbilitySO ability)
        {
            OnAbilityCast?.Invoke(ability);

            if (IsServerInitialized)
            {
                int slotIndex = System.Array.IndexOf(abilities, ability);
                if (slotIndex >= 0)
                {
                    // 🎯 Сразу же отправляем: способность кастнута (CD стартует)
                    RpcNotifyAbilityCast(slotIndex);
                    
                    // Обновляем последний синхронизированный CD
                    float newCd = service.GetCooldownRemaining(abilities[slotIndex]);
                    lastSyncedCooldowns[slotIndex] = newCd;
                }
            }
        }

        private void OnCooldownChangedHandler(AbilitySO ability, float remaining, float max)
        {
            OnCooldownChanged?.Invoke(ability, remaining, max);

            int slotIndex = System.Array.IndexOf(abilities, ability);
            if (IsServerInitialized && slotIndex >= 0)
            {
                cooldownValues[slotIndex] = remaining;
                
                // 🎯 Важный момент: CD закончился (remaining == 0)
                // Отправляем сразу, не ждём siguiente синхро
                if (remaining <= 0.01f && lastSyncedCooldowns[slotIndex] > 0.05f)
                {
                    lastSyncedCooldowns[slotIndex] = 0f;
                    RpcSyncCooldown(slotIndex, 0f);
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

        public float GetCooldown(int index)
        {
            if (!IsReady || index < 0 || index >= abilities.Length)
                return 0f;

            return cooldownValues[index] > 0 ? cooldownValues[index] : 0f;
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

        /// <summary>
        /// 🎯 Отправляем всем: способность была кастнута (критично сразу же)
        /// </summary>
        [ObserversRpc]
        private void RpcNotifyAbilityCast(int slotIndex)
        {
            Debug.Log($"[AbilityCaster] Ability cast at slot {slotIndex}");
            if (slotIndex >= 0 && slotIndex < abilities.Length && abilities[slotIndex] != null)
            {
                OnAbilityCast?.Invoke(abilities[slotIndex]);
            }
        }

        /// <summary>
        /// 🎯 Периодическая синхронизация CD (раз в 0.5 сек, только изменившиеся)
        /// </summary>
        [ObserversRpc]
        private void RpcSyncCooldown(int slotIndex, float cooldownValue)
        {
            if (slotIndex >= 0 && slotIndex < cooldownValues.Length)
            {
                cooldownValues[slotIndex] = cooldownValue;
                
                if (slotIndex < abilities.Length && abilities[slotIndex] != null)
                {
                    OnCooldownChanged?.Invoke(abilities[slotIndex], cooldownValue, 
                                            abilities[slotIndex].cooldown);
                }
            }
        }
    }
}
