using UnityEngine;
using FishNet;

public sealed class ClientLogin : MonoBehaviour
{
    private void Start()
    {
        string persistentId = SystemInfo.deviceUniqueIdentifier;

        var msg = new LoginMessage
        {
            PersistentId = persistentId
        };

        InstanceFinder.ClientManager.Broadcast(msg);
    }
}
