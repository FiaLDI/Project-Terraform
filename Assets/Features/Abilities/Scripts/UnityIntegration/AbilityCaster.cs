using System;
using System.Collections.Generic;
using Features.Abilities.Domain;
using Features.Abilities.UnityIntegration;
using Features.Buffs.Domain;
using Features.Stats.Domain;
using Features.Stats.UnityIntegration;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

namespace Features.Abilities.Application
{
    [DefaultExecutionOrder(-150)]
    [RequireComponent(typeof(ServerGamePhase))]
    [RequireComponent(typeof(PlayerStats))]
    public sealed class AbilityCaster : NetworkBehaviour
    {
        [Header("Ability slots")]
        [SerializeField] private AbilitySO[] abilities = new AbilitySO[5];
        public IReadOnlyList<AbilitySO> Abilities => abilities;

        [Header("Library")]
        [SerializeField] private AbilityLibrarySO abilityLibrary;

        [Header("Auto refs")]
        public LayerMask groundMask;

        // ================= NETWORK STATE =================

        public readonly SyncList<float> Cooldowns = new();

        public readonly SyncVar<bool> NetIsChanneling = new();
        public readonly SyncVar<int> NetChannelSlot = new();
        public readonly SyncVar<float> NetChannelRemaining = new();

        // ================= RUNTIME =================

        private IEnergyStats energy;
        private AbilityService service;
        private ServerGamePhase phase;
        private IBuffSource buffSource;

        public bool IsReady { get; private set; }

        // ================= EVENTS (CLIENT SIDE) =================

        public event Action OnAbilitiesChanged;
        public event Action<AbilitySO, float, float> OnCooldownChanged;
        public event Action<AbilitySO> OnChannelStarted;
        public event Action<AbilitySO, float, float> OnChannelProgress;
        public event Action<AbilitySO> OnChannelCompleted;
        private int lastChannelSlot = -1;

        // ================= EVENTS =================

        public event Action<AbilitySO> OnAbilityCast;
        public event Action<AbilitySO> OnChannelInterrupted;

        // =====================================================

        [SerializeField] private bool debugCooldown = false;

        private void Awake()
        {
            phase = GetComponent<ServerGamePhase>();
            buffSource = GetComponent<IBuffSource>();

            Cooldowns.OnChange += OnCooldownSync;
            NetIsChanneling.OnChange += OnChannelStateChanged;
            NetChannelRemaining.OnChange += OnChannelRemainingChanged;
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            AbilityTickSystem.Register(this);
            phase.OnPhaseReached += OnPhaseReached;
        }

        public override void OnStopServer()
        {
            AbilityTickSystem.Unregister(this);
            phase.OnPhaseReached -= OnPhaseReached;
            base.OnStopServer();
        }

        // ================= SERVER TICK =================

        public void ServerTick(float dt)
        {
            if (!IsReady || service == null)
                return;

            service.Tick(dt);

            for (int i = 0; i < abilities.Length; i++)
            {
                if (abilities[i] == null)
                    continue;



                if (debugCooldown && IsServer)
                {
                    Debug.Log(
                        $"[SERVER] Player={OwnerId} Slot={i} CD={Cooldowns[i]:F2}",
                        this
                    );
                }

                Cooldowns[i] = service.GetCooldownRemaining(abilities[i]);
            }

            if (service.IsChanneling)
            {
                NetIsChanneling.Value = true;
                NetChannelSlot.Value = Array.IndexOf(abilities, service.CurrentChannelAbility);
                NetChannelRemaining.Value = service.GetChannelRemaining();
            }
            else
            {
                NetIsChanneling.Value = false;
                NetChannelSlot.Value = -1;
                NetChannelRemaining.Value = 0f;
            }
        }

        private void OnCooldownSync(SyncListOperation op, int index, float oldValue, float newValue, bool asServer)
        {
            if (asServer)
                return;

            if (index < 0 || index >= abilities.Length)
                return;

            var ability = abilities[index];
            if (ability == null)
                return;
            
            if (debugCooldown && !asServer)
            {
                Debug.Log(
                    $"[CLIENT] Player={OwnerId} Slot={index} CD={newValue:F2}",
                    this
                );
            }

            OnCooldownChanged?.Invoke(ability, newValue, ability.cooldown);
        }

        // ================= INIT =================

        private void OnPhaseReached(GamePhase p)
        {
            if (p == GamePhase.PassivesApplied && !IsReady)
                InitServer();
        }

        private void InitServer()
        {
            var stats = GetComponent<PlayerStats>();
            energy = stats?.Facade?.Energy;

            service = new AbilityService(
                owner: buffSource,
                energy: energy,
                groundMask: groundMask,
                executor: AbilityExecutor.Instance
            );

            IsReady = true;

            Cooldowns.Clear();
            for (int i = 0; i < abilities.Length; i++)
                Cooldowns.Add(0f);

            OnAbilitiesChanged?.Invoke();
            phase.Reach(GamePhase.AbilitiesReady);
        }

        // ================= API =================

        public bool TryCastWithContext(int index, out AbilitySO ability, out AbilityContext ctx)
        {
            ability = null;
            ctx = default;

            if (!IsReady)
                return false;

            if (index < 0 || index >= abilities.Length)
                return false;

            ability = abilities[index];
            if (ability == null)
                return false;

            bool ok = service.TryCast(ability, index);
            if (!ok)
                return false;
            
            OnAbilityCast?.Invoke(ability);

            ctx = ability.castType == AbilityCastType.Instant
                ? service.LastInstantContext
                : service.LastChannelContext;

            return true;
        }

        private void OnChannelStateChanged(bool prev, bool next, bool asServer)
        {
            if (!IsClient)
                return;

            if (next && NetChannelSlot.Value >= 0)
            {
                lastChannelSlot = NetChannelSlot.Value;
                OnChannelStarted?.Invoke(abilities[lastChannelSlot]);
                return;
            }

            if (prev && !next)
            {
                if (lastChannelSlot >= 0)
                    OnChannelCompleted?.Invoke(abilities[lastChannelSlot]);

                lastChannelSlot = -1;
            }
        }

        private void OnChannelRemainingChanged(float prev, float next, bool asServer)
        {
            if (!IsClient)
                return;

            if (!NetIsChanneling.Value || NetChannelSlot.Value < 0)
                return;

            var ability = abilities[NetChannelSlot.Value];
            float total = ability.castTime;
            float elapsed = total - next;

            OnChannelProgress?.Invoke(ability, elapsed, total);
        }

        public void SetAbilities(AbilitySO[] newAbilities)
        {
            for (int i = 0; i < abilities.Length; i++)
            {
                abilities[i] = (newAbilities != null && i < newAbilities.Length)
                    ? newAbilities[i]
                    : null;
            }

            if (IsReady)
                OnAbilitiesChanged?.Invoke();
        }
    }
}
