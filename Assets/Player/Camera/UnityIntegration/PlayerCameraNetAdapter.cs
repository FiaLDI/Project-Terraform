using FishNet.Connection;
using FishNet.Object;
using UnityEngine;

namespace Features.Player.UnityIntegration
{
    public sealed class PlayerCameraNetAdapter : NetworkBehaviour
    {
        // private PlayerCameraController controller;

        // public override void OnStartClient()
        // {
        //     base.OnStartClient();

        //     controller = GetComponent<PlayerCameraController>();

        //     Debug.Log($"[camera-fix] {name} OnStartClient | IsOwner={IsOwner}");

        //     if (controller == null)
        //     {
        //         Debug.LogError($"[camera-fix] {name} MISSING PlayerCameraController");
        //         return;
        //     }

        //     controller.SetLocal(IsOwner);
        // }

        // public override void OnOwnershipClient(NetworkConnection prevOwner)
        // {
        //     Debug.Log($"[CameraNet] Ownership changed on {name} | IsOwner={IsOwner}");
        //     controller?.SetLocal(IsOwner);
        // }

        // public override void OnStopClient()
        // {
        //     if (IsOwner)
        //         controller?.SetLocal(false);
        // }
    }
}
