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
        base.OnStartClient();

        if (!IsOwner)
            return;

        var progress = PlayerProgressService.Instance;
        var character = progress?.GetActiveCharacter();

        if (character?.characterInventoryData != null)
        {
            SendInventoryToServer(character.characterInventoryData);
        }
    }

    [ServerRpc]
    private void SendInventoryToServer(InventorySaveData data)
    {
        if (inventory == null)
            inventory = GetComponent<InventoryManager>();

        inventory.LoadFromSave(data);

        GetComponent<InventoryStateNetwork>()?.ForceSync();
    }
}
