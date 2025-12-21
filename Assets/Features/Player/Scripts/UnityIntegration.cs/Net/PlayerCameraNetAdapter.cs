using FishNet.Object;
using UnityEngine;

namespace Features.Player.UnityIntegration
{
    public sealed class PlayerCameraNetAdapter : NetworkBehaviour
    {
        private PlayerCameraController controller;

        private void Awake()
        {
            controller = GetComponent<PlayerCameraController>();
            Debug.Log($"[CameraNet][Awake] {name}");
        }

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();

            bool isLocal = base.Owner.IsLocalClient;

            Debug.Log(
                $"[CameraNet][OnStartNetwork] {name} | " +
                $"IsOwner={isLocal} IsClient={IsClient} IsServer={IsServer}"
            );

            controller.SetLocal(isLocal);
        }

        public override void OnStopClient()
        {
            Debug.Log($"[CameraNet][OnStopClient] {name}");
            controller.SetLocal(false);
        }
    }
}
