using Features.Abilities.Application;
using FishNet.Object;
using UnityEngine;

namespace Features.Player.UnityIntegration
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(AbilityCaster))]
    public sealed class AbilityCasterNetAdapter : NetworkBehaviour
    {
        private AbilityCaster caster;

        // =====================================================
        // LIFECYCLE
        // =====================================================

        private void Awake()
        {
            caster = GetComponent<AbilityCaster>();
        }

        public override void OnStartServer()
        {
            base.OnStartServer();

            if (caster == null)
                caster = GetComponent<AbilityCaster>();
        }

        // =====================================================
        // CLIENT → SERVER
        // =====================================================

        /// <summary>
        /// Вызывается ТОЛЬКО локальным игроком.
        /// Отправляет намерение каста на сервер.
        /// </summary>
        public void Cast(int index)
        {
            if (!IsOwner)
                return;

            // защита от гонок до инициализации
            if (!IsClientInitialized)
                return;

            Cast_Server(index);
        }

        // =====================================================
        // SERVER (AUTHORITATIVE)
        // =====================================================

        [ServerRpc]
        private void Cast_Server(int index)
        {
            if (caster == null)
                return;

            if (!caster.IsReady)
                return;

            // ❗ ВАЖНО:
            // Execute происходит ТОЛЬКО на сервере внутри AbilityService
            caster.TryCastWithContext(index, out _, out _);
        }
    }
}
