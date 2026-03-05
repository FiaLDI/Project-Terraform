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

        string pid = PersistentIdProvider.GetOrCreate();

        var active = PlayerProgressService.Instance.GetActiveCharacter();
        Debug.Log($"[LOGIN] Sending classId = {active.classId}");
        
        SendLoginServerRpc(
            pid,
            active.characterId,
            active.classId,
            active.level);
    }

    [ServerRpc]
    private void SendLoginServerRpc(
        string persistentId,
        string characterId,
        string classId,
        int level,
        NetworkConnection sender = null)
    {
        
        if (sender == null)
            return;

        ServerLoginHandler.I.HandleLogin(
            sender,
            persistentId,
            characterId,
            classId,
            level);
    }
}