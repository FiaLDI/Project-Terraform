using System.Collections;
using UnityEngine;
using Features.Abilities.Application;
using Features.Abilities.Client;
using Features.Abilities.Domain;
using Features.Classes.Data;
using Features.Stats.UnityIntegration;
using FishNet.Object;

namespace Features.Class.Net
{
    [RequireComponent(typeof(PlayerClassController))]
    [RequireComponent(typeof(PlayerStats))]
    [RequireComponent(typeof(ServerGamePhase))]
    [RequireComponent(typeof(AbilityCaster))]
    public sealed class PlayerStateNetAdapter : NetworkBehaviour
    {
        private PlayerClassController classController;
        private PlayerStats playerStats;
        private AbilityCaster abilityCaster;
        private ServerGamePhase phase;

        [SerializeField]
        private PlayerClassLibrarySO classLibrary;

        private bool classApplied;
        private bool abilitiesSent;
        private string pendingClassId;

        // =====================================================
        // LIFECYCLE
        // =====================================================

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
            abilityCaster ??= GetComponent<AbilityCaster>();
            phase ??= GetComponent<ServerGamePhase>();
        }

        // =====================================================
        // SERVER ENTRY POINT
        // =====================================================

        /// <summary>
        /// Единственный допустимый способ применить класс.
        /// Можно вызывать сразу после спавна.
        /// </summary>
        [Server]
        public void ApplyClass(string classId)
        {
            if (classApplied)
                return;

            pendingClassId = classId;

            if (phase.IsAtLeast(GamePhase.StatsReady))
                ApplyClassInternal();
        }

        // =====================================================
        // PHASE
        // =====================================================

        private void OnPhaseReached(GamePhase p)
        {
            if (p == GamePhase.StatsReady && !classApplied)
                ApplyClassInternal();
        }

        // =====================================================
        // PIPELINE
        // =====================================================

        [Server]
        private void ApplyClassInternal()
        {
            if (classApplied)
                return;

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

            classApplied = true;

            // 1️⃣ базовые статы
            playerStats.ResetAndApplyDefaults();

            // 2️⃣ пресет класса
            playerStats.ApplyPreset(cfg.preset);

            // 3️⃣ пассивы / бафы / server-side abilities
            classController.ApplyClass(pendingClassId);

            // 4️⃣ abilities → clients (РОВНО 1 РАЗ)
            StartCoroutine(SendAbilitiesOnce(cfg));

            Debug.Log(
                $"[PlayerStateNetAdapter] ✅ Class '{pendingClassId}' applied",
                this
            );
        }

        // =====================================================
        // ABILITIES SYNC (ONE-SHOT)
        // =====================================================

        [Server]
        private IEnumerator SendAbilitiesOnce(PlayerClassConfigSO cfg)
        {
            if (abilitiesSent)
                yield break;

            abilitiesSent = true;

            // гарантируем, что клиент уже инициализирован
            yield return null;
            yield return null;

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

        // =====================================================
        // CLIENT VIEW ONLY
        // =====================================================

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

            // 🔹 CLIENT VIEW (UI DATA ONLY)
            var view = GetComponent<ClientAbilityView>();
            if (view != null)
            {
                view.SetAbilities(loaded);
            }
            else
            {
                Debug.LogError(
                    "[PlayerStateNetAdapter] ClientAbilityView missing",
                    this
                );
            }

            // 🔹 CLIENT RUNTIME (cooldowns / channel visuals)
            // ⚠️ НЕ ВЫКЛЮЧАЕМ AbilityCaster!
            if (abilityCaster != null)
            {
                abilityCaster.SetAbilities(loaded);
            }
        }
    }
}
