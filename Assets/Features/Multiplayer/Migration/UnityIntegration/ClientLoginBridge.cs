using UnityEngine;
using FishNet.Object;
using FishNet.Connection;

public sealed class ClientLoginBridge : NetworkBehaviour
{
    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!IsOwner)
            return;

        Debug.Log("[LoginBridge] Owner ready");

        // ❌ НЕ логинимся здесь
        ClientConnectionController.I?.OnLoginBridgeReady(this);
    }

    public void SendLogin(string persistentId)
    {
        if (!IsOwner)
            return;

        Debug.Log("[fix-net] Sending login to server");
        LoginServerRpc(persistentId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void LoginServerRpc(string persistentId, NetworkConnection sender = null)
    {
        if (sender == null)
            return;

        ServerLoginHandler.I.HandleLogin(sender, persistentId);
    }

    [TargetRpc]
    public void NotifySpawnedTargetRpc(NetworkConnection conn)
    {
        Debug.Log("[fix-net] Server confirmed spawn");
        ClientConnectionController.I?.NotifySpawned();
    }
}
