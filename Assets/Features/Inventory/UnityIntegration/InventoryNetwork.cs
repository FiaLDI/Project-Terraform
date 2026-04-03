using Features.Inventory.UnityIntegration;
using FishNet.Object;
using UnityEngine;

public class InventoryNetwork : NetworkBehaviour
{
    private InventoryManager inventory;

    private void Awake()
    {
        inventory = GetComponent<InventoryManager>();
    }

    public override void OnStartClient()
    {
        if (!IsOwner)
            return;

        var data = inventory.BuildSaveData();
        SendInventoryToServer(data);
    }

    [ServerRpc]
    private void SendInventoryToServer(InventorySaveData data)
    {
        inventory.LoadFromSave(data);

        Debug.Log("[SERVER] Inventory loaded from client");
    }
}
