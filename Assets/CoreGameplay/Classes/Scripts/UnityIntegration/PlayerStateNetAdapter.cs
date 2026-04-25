using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using Features.Classes.Data;
using Features.Abilities.Application;
using Features.Abilities.Domain;
using Features.Stats.UnityIntegration;
using Features.Passives.UnityIntegration;
using Features.Passives.Domain;
using Features.Stats.Domain;
using Features.Passives.Net;
using Features.Abilities.Client;

namespace Features.Class.Net
{
    [RequireComponent(typeof(PlayerClassController))]
    [RequireComponent(typeof(PlayerStats))]
    [RequireComponent(typeof(ServerGamePhase))]
    [RequireComponent(typeof(AbilityCaster))]
    [RequireComponent(typeof(PassiveSystem))]
    public sealed class PlayerStateNetAdapter : NetworkBehaviour
    {
        private PlayerClassController classController;
        private PlayerStats playerStats;
        private IStatsOwner statsOwner;
        private AbilityCaster abilityCaster;
        private PassiveSystem passiveSystem;
        private ServerGamePhase phase;

        [SerializeField]
        private PlayerClassLibrarySO classLibrary;

        private bool hasAppliedClass;
        private string pendingClassId;
        private int abilitySyncVersion;

        private readonly List<string> runtimePassiveIds = new();

        private void Awake()
        {
            classLibrary ??=
                UnityEngine.Resources.Load<PlayerClassLibrarySO>(
                    "Databases/PlayerClassLibrary"
                );
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            Cache();

            phase.OnPhaseReached += OnPhaseReached;
        }

        public override void OnStopServer()
        {
            if (phase != null)
                phase.OnPhaseReached -= OnPhaseReached;

            base.OnStopServer();
        }

        private void Cache()
        {
            classController ??= GetComponent<PlayerClassController>();
            playerStats ??= GetComponent<PlayerStats>();
            statsOwner ??= GetComponent<IStatsOwner>();
            abilityCaster ??= GetComponent<AbilityCaster>();
            passiveSystem ??= GetComponent<PassiveSystem>();
            phase ??= GetComponent<ServerGamePhase>();

            if (statsOwner == null)
            {
                Debug.LogError(
                    "[PlayerStateNetAdapter] IStatsOwner not found",
                    this
                );
            }
        }

        [Server]
        public void PreInitProgression(IEnumerable<string> passiveIds)
        {
            runtimePassiveIds.Clear();

            if (passiveIds == null)
                return;

            foreach (var id in passiveIds)
            {
                if (!string.IsNullOrWhiteSpace(id) && !runtimePassiveIds.Contains(id))
                    runtimePassiveIds.Add(id);
            }
        }

        [Server]
        public void ApplyClass(string classId)
        {
            if (string.IsNullOrWhiteSpace(classId))
                return;

            pendingClassId = classId;

            if (phase.IsAtLeast(GamePhase.StatsReady))
                ApplyClassInternal();
        }

        private void OnPhaseReached(GamePhase p)
        {
            if (p == GamePhase.StatsReady && !string.IsNullOrEmpty(pendingClassId))
                ApplyClassInternal();
        }

        [Server]
        private void ApplyClassInternal()
        {
            if (statsOwner == null || !statsOwner.IsReady)
            {
                Debug.LogWarning(
                    "[PlayerStateNetAdapter] StatsOwner not ready yet",
                    this
                );
                return;
            }

            if (string.IsNullOrEmpty(pendingClassId))
            {
                Debug.LogError(
                    "[PlayerStateNetAdapter] No classId provided",
                    this
                );
                return;
            }

            var cfg = classLibrary.FindById(pendingClassId);
            if (cfg == null)
            {
                Debug.LogError(
                    $"[PlayerStateNetAdapter] Class '{pendingClassId}' not found",
                    this
                );
                return;
            }

            hasAppliedClass = true;

            playerStats.ResetAndApplyDefaults();
            playerStats.ApplyPreset(cfg.preset);

            var finalPassives = BuildPassives(cfg);
            var net = GetComponent<PassiveNetAdapter>();
            net.ServerSetPassives(finalPassives);

            classController.ApplyClass(pendingClassId);

            GetComponent<MovementStatsSync>()?.SendSnapshot();

            abilitySyncVersion++;
            StartCoroutine(SendAbilities(cfg, abilitySyncVersion));

            Debug.Log(
                $"[PlayerStateNetAdapter] Class '{pendingClassId}' applied",
                this
            );
        }

        [Server]
        private PassiveSO[] BuildPassives(PlayerClassConfigSO cfg)
        {
            var list = new List<PassiveSO>();

            if (cfg.passives != null)
                list.AddRange(cfg.passives);

            foreach (var id in runtimePassiveIds)
            {
                var p = Features.Passives.Data.PassiveRegistrySO.Instance.GetById(id);
                if (p != null)
                    list.Add(p);
            }

            return list.ToArray();
        }

        [Server]
        public void RefreshPassives()
        {
            if (!hasAppliedClass)
                return;

            var cfg = classLibrary.FindById(pendingClassId);
            if (cfg == null)
                return;

            var finalPassives = BuildPassives(cfg);

            var net = GetComponent<PassiveNetAdapter>();
            net.ServerSetPassives(finalPassives);

            Debug.Log("[PlayerStateNetAdapter] Passives refreshed", this);
        }

        [Server]
        private IEnumerator SendAbilities(PlayerClassConfigSO cfg, int syncVersion)
        {
            yield return null;
            yield return null;

            if (syncVersion != abilitySyncVersion)
                yield break;

            if (cfg.abilities == null || cfg.abilities.Count == 0)
            {
                RpcApplyAbilities(System.Array.Empty<string>());
                yield break;
            }

            var ids = new string[cfg.abilities.Count];
            for (int i = 0; i < cfg.abilities.Count; i++)
                ids[i] = cfg.abilities[i]?.id ?? string.Empty;

            RpcApplyAbilities(ids);
        }

        [ServerRpc(RequireOwnership = true)]
        public void RequestRefreshPassivesServerRpc()
        {
            RefreshPassives();
        }

        [ServerRpc(RequireOwnership = true)]
        public void ApplyClientProgressionServerRpc(string[] passiveIds)
        {
            runtimePassiveIds.Clear();

            if (passiveIds != null)
            {
                foreach (var id in passiveIds)
                {
                    if (!string.IsNullOrWhiteSpace(id) && !runtimePassiveIds.Contains(id))
                        runtimePassiveIds.Add(id);
                }
            }

            var session = ServerCompositionRoot.I?.Sessions?.GetSessionByClient(Owner.ClientId);
            session?.SetPassives(runtimePassiveIds);

            RefreshPassives();

            Debug.Log(
                $"[SERVER] Progression synced for owner {Owner.ClientId} ({runtimePassiveIds.Count} passives)",
                this
            );
        }

        [ObserversRpc]
        private void RpcApplyAbilities(string[] abilityIds)
        {
            Cache();

            var lib = UnityEngine.Resources.Load<AbilityLibrarySO>(
                "Databases/AbilityLibrary"
            );

            if (lib == null)
            {
                Debug.LogError(
                    "[PlayerStateNetAdapter] AbilityLibrary not found",
                    this
                );
                return;
            }

            var loaded = new AbilitySO[abilityIds.Length];
            for (int i = 0; i < abilityIds.Length; i++)
                loaded[i] = lib.FindById(abilityIds[i]);

            var view = GetComponent<ClientAbilityView>();
            if (view != null)
                view.SetAbilities(loaded);

            if (abilityCaster != null)
                abilityCaster.SetAbilities(loaded);
        }
    }
}
