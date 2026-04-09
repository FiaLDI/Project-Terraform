using FishNet.Object;
using UnityEngine;

public class PlayerEquipmentNetwork : NetworkBehaviour
{
    private PlayerNetworkController controller;

    private void Awake()
    {
        controller = GetComponent<PlayerNetworkController>();
    }

    public void SetWeaponPose(int pose)
    {
        controller?.SetWeaponPose(pose);

        if (!IsServer)
            SetWeaponPoseServerRpc(pose);
        else
            controller?.SetWeaponPose(pose);
    }

    [ServerRpc]
    private void SetWeaponPoseServerRpc(int pose)
    {
        controller?.SetWeaponPose(pose);
    }
}
