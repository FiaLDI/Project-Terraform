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
        private NetworkPlayer networkPlayer;

        private void Awake()
        {
            caster = GetComponent<AbilityCaster>();
            networkPlayer = GetComponent<NetworkPlayer>();
        }

        public override void OnStartServer()
        {
            base.OnStartServer();

            if (caster == null)
                caster = GetComponent<AbilityCaster>();

            if (networkPlayer == null)
                networkPlayer = GetComponent<NetworkPlayer>();
        }

        public void Cast(int index)
        {
            if (!IsOwner)
                return;

            if (networkPlayer != null && networkPlayer.IsDead)
                return;

            if (!IsClientInitialized)
                return;

            Cast_Server(index);
        }

        [ServerRpc]
        private void Cast_Server(int index)
        {
            if (caster == null)
                return;

            if (networkPlayer != null && networkPlayer.IsDead)
                return;

            if (!caster.IsReady)
                return;

            caster.TryCastWithContext(index, out _, out _);
        }
    }
}
