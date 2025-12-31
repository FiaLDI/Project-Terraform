using System.Collections;
using Features.Abilities.Application;
using Features.Abilities.Domain;
using Features.Buffs.Application;
using Features.Classes.Data;
using Features.Stats.UnityIntegration;
using FishNet.Object;
using UnityEngine;

namespace Features.Class.Net
{
    /// <summary>
    /// Серверный адаптер состояния игрока.
    ///
    /// ГАРАНТИИ:
    /// - Класс применяется ТОЛЬКО на сервере
    /// - ТОЛЬКО после готовности PlayerStats
    /// - В ОДНОМ месте
    /// - Клиент НЕ трогает статы
    /// </summary>
    [RequireComponent(typeof(PlayerClassController))]
    [RequireComponent(typeof(PlayerStats))]
    public sealed class PlayerStateNetAdapter : NetworkBehaviour
    {
        private PlayerClassController classController;
        private PlayerStats playerStats;
        private AbilityCaster abilityCaster;
        [SerializeField]
        private PlayerClassLibrarySO classLibrary;

        private bool _classApplied;

        private void Awake()
        {
            classLibrary = UnityEngine.Resources.Load<PlayerClassLibrarySO>("Databases/PlayerClassLibrary");
        }

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();
            Cache();
        }

        private void Cache()
        {
            if (classController == null)
                classController = GetComponent<PlayerClassController>();

            if (playerStats == null)
                playerStats = GetComponent<PlayerStats>();

            if (abilityCaster == null)
                abilityCaster = GetComponent<AbilityCaster>();
        }

        // =====================================================
        // SERVER ENTRY POINT (ЕДИНСТВЕННЫЙ)
        // =====================================================

        /// <summary>
        /// ЕДИНСТВЕННЫЙ допустимый способ применить класс.
        /// </summary>
        [Server]
        public void ApplyClassWhenReady(string classId)
        {
            if (_classApplied)
                return;

            StartCoroutine(WaitAndApply(classId));
        }

        private IEnumerator WaitAndApply(string classId)
        {
            Cache();

            while (playerStats == null || !playerStats.IsReady || playerStats.Facade == null)
                yield return null;

            ApplyClassInternal(classId);
        }

        // =====================================================
        // SERVER PIPELINE
        // =====================================================

        [Server]
        private void ApplyClassInternal(string classId)
        {
            if (_classApplied)
                return;
            if (playerStats.Facade == null)
            {
                Debug.LogError(
                    "[PlayerStateNetAdapter] Facade is null despite IsReady == true",
                    this
                );
                return;
            }

            _classApplied = true;

            var cfg = classLibrary.FindById(classId);
            if (cfg == null)
            {
                Debug.LogError($"[PlayerStateNetAdapter] Class '{classId}' not found", this);
                return;
            }

            // 1 default
            playerStats.ResetAndApplyDefaults();

            // 2 preset class
            playerStats.ApplyPreset(cfg.preset);

            // 3 passive / buff
            classController.ApplyClass(classId);

            // 4 ability → client
            StartCoroutine(SyncAbilitiesAfterClassApplied());

            Debug.Log($"[PlayerStateNetAdapter] ✅ Class '{classId}' applied on SERVER", this);
        }

        // =====================================================
        // ABILITIES SYNC
        // =====================================================

        [Server]
        private IEnumerator SyncAbilitiesAfterClassApplied()
        {
            yield return null; // дать всем системам зарегистрироваться

            var appliedClass = classController.GetCurrentClass();
            string[] abilityIds = System.Array.Empty<string>();

            if (appliedClass != null && appliedClass.abilities != null)
            {
                abilityIds = new string[appliedClass.abilities.Count];
                for (int i = 0; i < appliedClass.abilities.Count; i++)
                    abilityIds[i] = appliedClass.abilities[i]?.id ?? string.Empty;
            }

            RpcApplyAbilities(abilityIds);
        }

        // =====================================================
        // CLIENT SIDE (VIEW ONLY)
        // =====================================================

        [ObserversRpc]
        private void RpcApplyAbilities(string[] abilityIds)
        {
            Cache();

            // хост уже применил
            if (IsServerInitialized && !IsClientOnlyInitialized)
                return;

            if (abilityCaster == null)
                return;

            if (abilityIds != null && abilityIds.Length > 0)
            {
                var lib = UnityEngine.Resources.Load<AbilityLibrarySO>("Databases/AbilityLibrary");
                if (lib != null)
                {
                    var loaded = new AbilitySO[abilityIds.Length];
                    for (int i = 0; i < abilityIds.Length; i++)
                        loaded[i] = lib.FindById(abilityIds[i]);

                    abilityCaster.SetAbilities(loaded);
                }
            }
            else
            {
                abilityCaster.SetAbilities(System.Array.Empty<AbilitySO>());
            }

            if (!IsOwner)
                abilityCaster.enabled = false;
        }
    }
}
