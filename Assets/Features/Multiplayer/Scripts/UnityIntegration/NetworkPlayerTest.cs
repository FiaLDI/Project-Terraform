using FishNet.Object;
using UnityEngine;

public class NetworkPlayerTest : NetworkBehaviour
{
    public override void OnStartServer()
    {
        Debug.Log($"[NetworkPlayerTest] Server started object {ObjectId}");
    }

    public override void OnStartClient()
    {
        Debug.Log($"[NetworkPlayerTest] Client started object {ObjectId}, IsOwner={IsOwner}");
    }
}
