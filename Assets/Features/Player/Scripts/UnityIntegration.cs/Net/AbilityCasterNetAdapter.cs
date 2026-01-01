using Features.Abilities.Application;
using Features.Abilities.Domain;
using FishNet.Object;
using UnityEngine;

namespace Features.Player.UnityIntegration
{
    [DisallowMultipleComponent]
    public sealed class AbilityCasterNetAdapter : NetworkBehaviour
    {
        private AbilityCaster caster;

        private void Awake()
        {
            enabled = true;
            caster = GetComponent<AbilityCaster>();
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            enabled = true;
        }

        // ================= CLIENT =================
        // Вызывается ТОЛЬКО локальным игроком
        public void Cast(int index)
        {
            if (!IsOwner)
                return;

            Debug.Log($"[NetAdapter] CLIENT -> SERVER Cast({index})");
            Cast_Server(index);
        }

        // ================= SERVER =================
        [ServerRpc]
        private void Cast_Server(int index)
        {
            Debug.Log($"[NetAdapter] SERVER Received Cast({index})");

            if (caster == null || !caster.IsReady)
                return;

            // 🎯 ВАЖНО:
            // Execute произойдёт ТОЛЬКО внутри AbilityService (на сервере)
            caster.TryCastWithContext(index, out _, out _);
        }
    }
}
