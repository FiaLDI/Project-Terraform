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

        var progress = PlayerProgressService.Instance;
        if (progress == null)
        {
            Debug.LogError("[LOGIN] PlayerProgressService is missing");
            return;
        }

        var active = progress.GetActiveCharacter();
        if (active == null)
        {
            Debug.LogError("[LOGIN] Active character not found");
            return;
        }

        Debug.Log($"[LOGIN] Sending classId = {active.classId}");
        
        SendLoginServerRpc(
            pid,
            active.characterId,
            active.classId,
            active.level,
            active.experience);
    }

    [ServerRpc]
    private void SendLoginServerRpc(
        string persistentId,
        string characterId,
        string classId,
        int level,
        int experience,
        NetworkConnection sender = null)
    {
        
        if (sender == null)
            return;

        ServerLoginHandler.I.HandleLogin(
            sender,
            persistentId,
            characterId,
            classId,
            level,
            experience);
    }
}
