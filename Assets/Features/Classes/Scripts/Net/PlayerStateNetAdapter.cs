using FishNet.Object;
using UnityEngine;
using Features.Stats.UnityIntegration;
using System.Collections;
using Features.Abilities.Application;
using Features.Abilities.Domain;


namespace Features.Class.Net
{
    [RequireComponent(typeof(PlayerClassController))]
    public sealed class PlayerStateNetAdapter : NetworkBehaviour
    {
        private PlayerClassController classController;
        private PlayerStats playerStats;


        // =====================================================
        // LIFECYCLE
        // =====================================================

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();
            Cache();
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            if (classController == null)
                Cache();
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            if (classController == null)
                Cache();
        }

        private void Cache()
        {
            if (classController == null)
            {
                classController = GetComponent<PlayerClassController>();
                if (classController == null)
                    Debug.LogError("[PlayerStateNetAdapter] PlayerClassController not found on " + gameObject.name, this);
            }

            if (playerStats == null)
            {
                playerStats = GetComponent<PlayerStats>();
                if (playerStats == null)
                    Debug.LogError("[PlayerStateNetAdapter] PlayerStats not found on " + gameObject.name, this);
            }

            Debug.Log($"[PlayerStateNetAdapter] Cache done. classController={classController != null}, playerStats={playerStats != null}", this);
        }

        private void OnEnable()
        {
            if (classController != null)
                classController.OnClassApplied += HandleClassApplied;
        }

        private void OnDisable()
        {
            if (classController != null)
                classController.OnClassApplied -= HandleClassApplied;
        }


        [Server]
        public void ApplyClass(string classId)
        {
            Debug.Log($"[PlayerStateNetAdapter-Server] ApplyClass({classId})", this);

            // ✅ Вызов локально на сервере
            classController.ApplyClass(classId);

            // ✅ Получение класса после применения
            StartCoroutine(WaitAndSyncAbilities(classId));
        }

        [Server]
        private IEnumerator WaitAndSyncAbilities(string classId)
        {
            // Ждем пока класс применится
            yield return new WaitForEndOfFrame();

            var appliedClass = classController.GetCurrentClass();
            if (appliedClass != null && appliedClass.abilities != null && appliedClass.abilities.Count > 0)
            {
                string[] abilityIds = new string[appliedClass.abilities.Count];
                for (int i = 0; i < appliedClass.abilities.Count; i++)
                {
                    abilityIds[i] = appliedClass.abilities[i]?.id ?? "";
                    Debug.Log($"[PlayerStateNetAdapter-Server] Ability[{i}]: {abilityIds[i]}", this);
                }

                Debug.Log($"[PlayerStateNetAdapter-Server] Calling RPC with {abilityIds.Length} abilities", this);
                
                // ✅ Вызов RPC на клиентов
                RpcApplyClassWithAbilities(classId, abilityIds);
            }
            else
            {
                Debug.LogWarning("[PlayerStateNetAdapter-Server] No abilities in class", this);
                RpcApplyClassWithAbilities(classId, new string[0]);
            }
        }

        [ObserversRpc]
        public void RpcApplyClassWithAbilities(string classId, string[] abilityIds)
        {
            // На сервере не применяем повторно
            if (IsServer && !IsClientOnly)
            {
                Debug.Log($"[PlayerStateNetAdapter-Server] Skipping RPC application (already applied on server)", this);
                return;
            }

            if (classController == null)
            {
                Debug.LogError("[PlayerStateNetAdapter] RpcApplyClass: classController is null", this);
                Cache();
                if (classController == null) return;
            }

            if (playerStats == null)
            {
                Debug.LogError("[PlayerStateNetAdapter] RpcApplyClass: playerStats is null", this);
                Cache();
                if (playerStats == null) return;
            }

            if (!playerStats.IsReady)
            {
                Debug.LogWarning("[PlayerStateNetAdapter] Stats not ready, initializing", this);
                playerStats.Init();
            }

            Debug.Log($"[PlayerStateNetAdapter-Client] RpcApplyClassWithAbilities({classId}), abilities={abilityIds?.Length ?? 0}", this);

            // 🟢 Клиент применяет класс
            classController.ApplyClass(classId);
            
            // 🟢 ВАЖНО: Загружаем абилити по ID и переопределяем
            var caster = GetComponent<AbilityCaster>();
            if (caster != null && abilityIds != null && abilityIds.Length > 0)
            {
                // Загружаем AbilityLibrary
                var abilityLibrary = UnityEngine.Resources.Load<AbilityLibrarySO>("Databases/AbilityLibrary");
                if (abilityLibrary == null)
                {
                    Debug.LogError("[PlayerStateNetAdapter] AbilityLibrary not found!", this);
                    return;
                }

                // 🟢 Загружаем абилити по ID
                AbilitySO[] loadedAbilities = new AbilitySO[abilityIds.Length];
                for (int i = 0; i < abilityIds.Length; i++)
                {
                    loadedAbilities[i] = abilityLibrary.FindById(abilityIds[i]);
                    Debug.Log($"[PlayerStateNetAdapter-Client] Loaded ability[{i}]: {abilityIds[i]} = {loadedAbilities[i]?.name ?? "null"}", this);
                }

                // 🟢 Устанавливаем загруженные абилити
                Debug.Log($"[PlayerStateNetAdapter-Client] Setting {loadedAbilities.Length} abilities to caster", this);
                caster.SetAbilities(loadedAbilities);
                
                Debug.Log($"[PlayerStateNetAdapter-Client] ✅ Set {loadedAbilities.Length} abilities to caster", this);
            }
            else if (caster != null && (abilityIds == null || abilityIds.Length == 0))
            {
                // 🟢 Если абилити пусто - очищаем
                Debug.LogWarning("[PlayerStateNetAdapter-Client] No abilities in class, clearing caster abilities", this);
                caster.SetAbilities(new AbilitySO[0]);
            }

            if (!IsOwner)
            {
                var abilityCaster = GetComponent<AbilityCaster>();
                if (abilityCaster != null)
                {
                    abilityCaster.enabled = false;
                    Debug.Log($"[PlayerStateNetAdapter] Disabled AbilityCaster for remote player", this);
                }
            }

            StartCoroutine(VerifyAbilitiesWithDelay());
        }

        private IEnumerator VerifyAbilitiesWithDelay()
        {
            yield return new WaitForEndOfFrame();

            var caster = GetComponent<AbilityCaster>();
            if (caster != null)
            {
                Debug.Log($"[PlayerStateNetAdapter-Client] ✅ Caster has {caster.Abilities.Count} abilities", this);
                
                if (caster.Abilities.Count > 0)
                {
                    for (int i = 0; i < caster.Abilities.Count; i++)
                    {
                        var ability = caster.Abilities[i];
                        Debug.Log($"  abilities[{i}] = {ability?.name ?? "null"}", this);
                    }
                }
                else
                {
                    Debug.LogWarning($"[PlayerStateNetAdapter-Client] ⚠️ Caster has NO abilities!", this);
                }
            }
            else
            {
                Debug.LogError("[PlayerStateNetAdapter-Client] ❌ AbilityCaster not found!", this);
            }
        }



        // =====================================================
        // EVENT HANDLER: After Class Applied
        // =====================================================

        private void HandleClassApplied()
        {
            Debug.Log("[PlayerStateNetAdapter] Class applied event fired", this);

            if (IsServer)
            {
                StartCoroutine(ValidateAndSyncStats());
            }
        }


        // =====================================================
        // SERVER: Validate & Sync Stats
        // =====================================================

        [Server]
        private IEnumerator ValidateAndSyncStats()
        {
            yield return new WaitForSeconds(0.1f);

            if (playerStats?.IsReady != true)
            {
                Debug.LogWarning("[PlayerStateNetAdapter] Stats not ready for validation", this);
                yield break;
            }

            var facade = playerStats.Facade;
            if (facade == null)
                yield break;

            Debug.Log($"[PlayerStateNetAdapter-Server] Validating stats...", this);

            var health = facade.Health;
            var energy = facade.Energy;

            Debug.Log(
                $"[Server Stats] HP={health.CurrentHp:0.0}/{health.MaxHp:0.0} " +
                $"EN={energy.CurrentEnergy:0.0}/{energy.MaxEnergy:0.0}",
                this
            );

            RpcSyncStats(
                health.CurrentHp,
                health.MaxHp,
                energy.CurrentEnergy,
                energy.MaxEnergy
            );
        }


        // =====================================================
        // CLIENT: Receive & Verify Stats
        // =====================================================

        [ObserversRpc]
        private void RpcSyncStats(
            float serverCurrentHp, float serverMaxHp,
            float serverCurrentEnergy, float serverMaxEnergy)
        {
            if (playerStats?.IsReady != true)
            {
                Debug.LogWarning("[PlayerStateNetAdapter] Stats not ready in RpcSyncStats", this);
                return;
            }

            var facade = playerStats.Facade;
            if (facade == null)
                return;

            var health = facade.Health;
            var energy = facade.Energy;

            bool hpMismatch = Mathf.Abs(health.MaxHp - serverMaxHp) > 0.01f;
            bool enMismatch = Mathf.Abs(energy.MaxEnergy - serverMaxEnergy) > 0.01f;

            if (hpMismatch)
            {
                Debug.LogWarning(
                    $"[PlayerStateNetAdapter-Client] HP MISMATCH! " +
                    $"Client={health.MaxHp:0.0} vs Server={serverMaxHp:0.0}",
                    this
                );

                health.SetCurrentHp(serverCurrentHp);
                health.SetMaxHpDirect(serverMaxHp);
            }

            if (enMismatch)
            {
                Debug.LogWarning(
                    $"[PlayerStateNetAdapter-Client] ENERGY MISMATCH! " +
                    $"Client={energy.MaxEnergy:0.0} vs Server={serverMaxEnergy:0.0}",
                    this
                );

                energy.SetCurrentEnergy(serverCurrentEnergy);
                energy.SetMaxEnergyDirect(serverMaxEnergy);
            }

            Debug.Log(
                $"[Client Stats] HP={health.CurrentHp:0.0}/{health.MaxHp:0.0} " +
                $"EN={energy.CurrentEnergy:0.0}/{energy.MaxEnergy:0.0}",
                this
            );
        }
    }
}
