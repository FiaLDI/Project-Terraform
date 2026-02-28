using FishNet.Object;
using FishNet.Connection;
using UnityEngine;

public sealed class ConnectionObject : NetworkBehaviour
{
    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!IsOwner)
            return;

        Debug.Log("[ConnectionObject] Owner ready, sending login");

        string pid = PersistentIdProvider.GetOrCreate();
        SendLoginServerRpc(pid);
    }

    [ServerRpc]
    private void SendLoginServerRpc(string persistentId, NetworkConnection sender = null)
    {
        if (sender == null)
            return;

        Debug.Log($"[ConnectionObject] Login RPC from {sender.ClientId}");

        ServerLoginHandler.I.HandleLogin(sender, persistentId);
    }
}