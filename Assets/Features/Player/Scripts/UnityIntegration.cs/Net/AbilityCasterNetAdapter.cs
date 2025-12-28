using Features.Abilities.Application;
using Features.Abilities.Domain;
using FishNet.Object;
using UnityEngine;

namespace Features.Player.UnityIntegration
{
    // Гарантирует, что NetworkBehaviour не выключится случайно
    [DisallowMultipleComponent] 
    public sealed class AbilityCasterNetAdapter : NetworkBehaviour
    {
        private AbilityCaster caster;

        private void Awake()
        {
            // 1. ЗАЩИТА: Принудительно включаем компонент
            this.enabled = true; 
            caster = GetComponent<AbilityCaster>();
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            // 2. ЗАЩИТА: На сервере тоже включаем
            this.enabled = true; 
        }

        // ===== INPUT (Вызывает Клиент) =====
        public void Cast(int index)
        {
            if (!IsOwner) return;
            // Лог отправки
            Debug.Log($"[NetAdapter] CLIENT: Sending Cast({index})..."); 
            Cast_Server(index);
        }

        // ===== SERVER =====
        [ServerRpc] // Убрали RequireOwnership = false, вернем как было, если включение поможет
        public void Cast_Server(int index)
        {
            // Если вы видите этот лог - победа
            Debug.Log($"[NetAdapter] SERVER: RPC Received! Slot: {index}");

            if (caster == null || !caster.IsReady) return;

            if (caster.TryCastWithContext(index, out AbilitySO ability, out AbilityContext ctx))
            {
                Cast_Client(index, ability.id, ctx);
            }
        }

        // ===== CLIENTS =====
        [ObserversRpc]
        public void Cast_Client(int index, string abilityId, AbilityContext ctx)
        {
            if (caster != null) 
            {
                var ability = caster.FindAbilityById(abilityId);
                if (ability != null) caster.PlayRemoteCast(ability, index, ctx);
            }
        }
    }
}
