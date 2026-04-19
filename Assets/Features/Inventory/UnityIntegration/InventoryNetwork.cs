using Features.Inventory.UnityIntegration;
using FishNet.Object;
using UnityEngine;

public class InventoryNetwork : NetworkBehaviour
{
    private bool sent;

    public void SendInitialInventoryToServer()
    {
        if (!IsOwner || sent)
            return;

        var progress = PlayerProgressService.Instance;
        if (progress == null)
        {
            Debug.LogWarning("[Inventory] No progress service yet");
            return;
        }

        var character = progress.GetActiveCharacter();
        var data = character?.characterInventoryData;

        if (data == null)
        {
            Debug.LogWarning("[Inventory] No save data");
            return;
        }

        sent = true;

        Debug.Log("[Inventory] Sending initial inventory to server");

        SendInventoryToServer(data);
    }

    [ServerRpc]
    private void SendInventoryToServer(InventorySaveData data)
    {
        var state = GetComponent<InventoryStateNetwork>();
        if (state == null)
            return;

        if (data == null)
        {
            Debug.LogWarning("[Inventory] NULL data ignored");
            return;
        }

        // 🔥 FIX: безопасная проверка
        bool isEmpty =
            (data.bag == null || data.bag.Count == 0) &&
            data.leftHand == null &&
            data.rightHand == null;

        if (isEmpty)
        {
            Debug.LogWarning("[Inventory] Empty initial data ignored");
            return;
        }

        var root = ServerCompositionRoot.I;
        var session = root?.Sessions?.GetSessionByClient(Owner.ClientId);

        if (session == null)
            return;

        // 🔥 источник истины = session
        if (!session.HasInventory)
        {
            session.SetInventory(data);
        }

        // 🔥 применяем напрямую
        state.ApplyInitialInventory(data);
    }
}
