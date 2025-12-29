using FishNet.Object;
using UnityEngine;
using Features.Stats.UnityIntegration;
using System.Collections;
using Features.Abilities.Application;
using Features.Abilities.Domain;
using Features.Buffs.Application; // Добавляем namespace для BuffSystem

namespace Features.Class.Net
{
    [RequireComponent(typeof(PlayerClassController))]
    public sealed class PlayerStateNetAdapter : NetworkBehaviour
    {
        private PlayerClassController classController;
        private PlayerStats playerStats;
        private BuffSystem buffSystem; // Ссылка на систему баффов

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();
            Cache();
        }

        private void Cache()
        {
            if (classController == null) classController = GetComponent<PlayerClassController>();
            if (playerStats == null) playerStats = GetComponent<PlayerStats>();
            if (buffSystem == null) buffSystem = GetComponent<BuffSystem>(); // Кэшируем BuffSystem
        }

        // --- SERVER SIDE ---

        [Server]
        public void ApplyClass(string classId)
        {
            if (classController == null) Cache();

            // 1. ФИКС СТАТОВ: ОЧИСТКА ПЕРЕД ПРИМЕНЕНИЕМ
            // Если мы применим класс поверх старого без очистки, статы удвоятся.
            if (buffSystem != null && buffSystem.ServiceReady)
            {
                Debug.Log("[PlayerStateNetAdapter] Clearing ALL buffs before applying class...", this);
                buffSystem.Service.ClearAll(); // Сбрасываем все статы в 0/базу
            }

            // 2. Применяем класс (накладываем баффы заново)
            Debug.Log($"[PlayerStateNetAdapter] Applying Class '{classId}' on Server", this);
            classController.ApplyClass(classId);

            // 3. Синхронизируем абилки
            StartCoroutine(WaitAndSyncAbilities(classId));
        }

        [Server]
        private IEnumerator WaitAndSyncAbilities(string classId)
        {
            yield return new WaitForEndOfFrame();
            
            var appliedClass = classController.GetCurrentClass();
            string[] abilityIds = new string[0];

            if (appliedClass != null && appliedClass.abilities != null)
            {
                abilityIds = new string[appliedClass.abilities.Count];
                for (int i = 0; i < appliedClass.abilities.Count; i++)
                    abilityIds[i] = appliedClass.abilities[i]?.id ?? "";
            }

            RpcApplyClassWithAbilities(classId, abilityIds);
            
            // Заодно форсируем синхронизацию статов, чтобы клиент увидел правильные цифры
            StartCoroutine(ValidateAndSyncStats());
        }

        // --- CLIENT SIDE ---

        [ObserversRpc]
        public void RpcApplyClassWithAbilities(string classId, string[] abilityIds)
        {
            if (IsServer && !IsClientOnly) return; // Хост уже все сделал

            Cache();
            if (playerStats != null && !playerStats.IsReady) playerStats.Init();

            // На клиенте тоже желательно почистить баффы, если система рассинхронизировалась
            if (buffSystem != null && buffSystem.ServiceReady)
            {
                buffSystem.Service.ClearAll();
            }

            // Применяем класс
            if (classController != null) classController.ApplyClass(classId);

            // Абилки...
            var caster = GetComponent<AbilityCaster>();
            if (caster != null)
            {
                if (abilityIds != null && abilityIds.Length > 0)
                {
                    var lib = UnityEngine.Resources.Load<AbilityLibrarySO>("Databases/AbilityLibrary");
                    if (lib != null)
                    {
                        var loaded = new AbilitySO[abilityIds.Length];
                        for(int i=0; i<abilityIds.Length; i++) loaded[i] = lib.FindById(abilityIds[i]);
                        caster.SetAbilities(loaded);
                    }
                }
                else
                {
                    caster.SetAbilities(new AbilitySO[0]);
                }
                if (!IsOwner) caster.enabled = false;
            }
        }

        [Server]
        private IEnumerator ValidateAndSyncStats()
        {
            yield return new WaitForSeconds(0.2f); // Небольшая задержка, чтобы баффы "устаканились"
            if (playerStats?.IsReady != true || playerStats.Facade == null) yield break;

            RpcSyncStats(
                playerStats.Facade.Health.CurrentHp,
                playerStats.Facade.Health.MaxHp,
                playerStats.Facade.Energy.CurrentEnergy,
                playerStats.Facade.Energy.MaxEnergy
            );
        }

        [ObserversRpc]
        private void RpcSyncStats(float curHp, float maxHp, float curEn, float maxEn)
        {
            if (IsServer && !IsClientOnly) return;
            if (playerStats?.IsReady != true) return;

            // Жесткая синхронизация
            var health = playerStats.Facade.Health;
            var energy = playerStats.Facade.Energy;

            if (Mathf.Abs(health.MaxHp - maxHp) > 0.1f) health.SetMaxHpDirect(maxHp);
            health.SetCurrentHp(curHp);

            if (Mathf.Abs(energy.MaxEnergy - maxEn) > 0.1f) energy.SetMaxEnergyDirect(maxEn);
            energy.SetCurrentEnergy(curEn);
        }
    }
}
