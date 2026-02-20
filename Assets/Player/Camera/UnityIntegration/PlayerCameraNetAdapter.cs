using FishNet.Connection;
using FishNet.Object;
using UnityEngine;

namespace Features.Player.UnityIntegration
{
    public sealed class PlayerCameraNetAdapter : NetworkBehaviour
    {
        private PlayerCameraController controller;

        public override void OnStartClient()
        {
            base.OnStartClient();

            controller = GetComponent<PlayerCameraController>();

            if (controller == null)
            {
                Debug.LogError($"[CameraNet] Missing PlayerCameraController on {name}");
                return;
            }

            Debug.Log($"[CameraNet] {name} OnStartClient | IsOwner={IsOwner}");

            controller.SetLocal(IsOwner);
        }

        public override void OnOwnershipClient(NetworkConnection prevOwner)
        {
            Debug.Log($"[CameraNet] Ownership changed on {name} | IsOwner={IsOwner}");
            controller?.SetLocal(IsOwner);
        }

        public override void OnStopClient()
        {
            if (controller != null)
                controller.SetLocal(false);
        }
    }
}
