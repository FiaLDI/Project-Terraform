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

        ClientConnectionController.I?.OnLoginBridgeReady(this);
    }

    public void SendLogin(string persistentId)
    {
        if (!IsOwner)
            return;

        LoginServerRpc(persistentId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void LoginServerRpc(string persistentId, NetworkConnection sender = null)
    {
        if (sender == null)
        {
            Debug.LogError("LoginServerRpc sender NULL");
            return;
        }

        ServerLoginHandler.I.HandleLogin(sender, persistentId);
    }

    [TargetRpc]
    public void NotifySpawnedTargetRpc(NetworkConnection conn)
    {
        ClientConnectionController.I?.NotifySpawned();
    }
}
