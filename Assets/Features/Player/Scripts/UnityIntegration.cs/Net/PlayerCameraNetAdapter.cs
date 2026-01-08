using FishNet.Object;
using UnityEngine;

namespace Features.Player.UnityIntegration
{
    public sealed class PlayerCameraNetAdapter : NetworkBehaviour
    {
        private PlayerCameraController controller;

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();

            controller = GetComponent<PlayerCameraController>();

            if (controller == null)
            {
                Debug.LogError($"[CameraNet] PlayerCameraController missing on {name}");
                return;
            }

            bool isLocal = Owner != null && Owner.IsLocalClient;
            controller.SetLocal(isLocal);
        }

        public override void OnStopClient()
        {
            if (controller != null)
                controller.SetLocal(false);
        }
    }
}
