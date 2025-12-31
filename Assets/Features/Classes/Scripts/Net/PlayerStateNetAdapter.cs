using FishNet.Object;
using UnityEngine;
using System.Collections;
using Features.Abilities.Application;
using Features.Abilities.Domain;
using Features.Buffs.Application;

namespace Features.Class.Net
{
    /// <summary>
    /// Сетевой адаптер состояния игрока.
    ///
    /// ВАЖНО:
    /// - НЕ синхронизирует HP / Energy вручную
    /// - НЕ вызывает PlayerStats.Init()
    /// - НЕ трогает StatsFacade напрямую на клиенте
    ///
    /// Статы синхронизируются ТОЛЬКО через StatsNetSync.
    /// </summary>
    [RequireComponent(typeof(PlayerClassController))]
    public sealed class PlayerStateNetAdapter : NetworkBehaviour
    {
        private PlayerClassController classController;
        private BuffSystem buffSystem;
        private AbilityCaster abilityCaster;

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();
            Cache();
        }

        private void Cache()
        {
            if (classController == null)
                classController = GetComponent<PlayerClassController>();

            if (buffSystem == null)
                buffSystem = GetComponent<BuffSystem>();

            if (abilityCaster == null)
                abilityCaster = GetComponent<AbilityCaster>();
        }

        // =====================================================
        // SERVER SIDE
        // =====================================================

        /// <summary>
        /// Применение класса игрока.
        /// ВЫЗЫВАЕТСЯ ТОЛЬКО НА СЕРВЕРЕ.
        /// </summary>
        [Server]
        public void ApplyClass(string classId)
        {
            Cache();

            // 1️⃣ Очищаем баффы ПЕРЕД применением класса
            if (buffSystem != null && buffSystem.ServiceReady)
            {
                Debug.Log("[PlayerStateNetAdapter] Clearing buffs before applying class", this);
                buffSystem.ClearAll();
            }

            // 2️⃣ Применяем класс (накладывает баффы на сервере)
            Debug.Log($"[PlayerStateNetAdapter] Applying class '{classId}' on SERVER", this);
            classController.ApplyClass(classId);

            // 3️⃣ Синхронизируем способности
            StartCoroutine(SyncAbilitiesAfterClassApplied(classId));
        }

        [Server]
        private IEnumerator SyncAbilitiesAfterClassApplied(string classId)
        {
            // Ждём кадр, чтобы класс гарантированно применился
            yield return new WaitForEndOfFrame();

            var appliedClass = classController.GetCurrentClass();
            string[] abilityIds = new string[0];

            if (appliedClass != null && appliedClass.abilities != null)
            {
                abilityIds = new string[appliedClass.abilities.Count];
                for (int i = 0; i < appliedClass.abilities.Count; i++)
                    abilityIds[i] = appliedClass.abilities[i]?.id ?? string.Empty;
            }

            RpcApplyClassWithAbilities(classId, abilityIds);
        }

        // =====================================================
        // CLIENT SIDE
        // =====================================================

        /// <summary>
        /// Клиент:
        /// - применяет класс ЛОКАЛЬНО (визуал / пассивки / описания)
        /// - обновляет способности
        ///
        /// ❗ НЕ меняет статы напрямую
        /// </summary>
        [ObserversRpc]
        private void RpcApplyClassWithAbilities(string classId, string[] abilityIds)
        {
            Cache();

            // Хост уже всё сделал на сервере
            if (IsServerInitialized && !IsClientOnlyInitialized)
                return;

            Debug.Log($"[PlayerStateNetAdapter] Applying class '{classId}' on CLIENT", this);

            // Применяем класс локально (без прямого изменения статов)
            if (classController != null)
                classController.ApplyClass(classId);

            // Обновляем способности
            if (abilityCaster != null)
            {
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

                // Удалённым игрокам кастер не нужен
                if (!IsOwner)
                    abilityCaster.enabled = false;
            }
        }
    }
}
