using System.Collections;
using FishNet.Object;
using UnityEngine;
using static ServerCompositionRoot;

public sealed class WorldReadyStatic : NetworkBehaviour
{
    private bool signaled;

    public override void OnStartServer()
    {
        base.OnStartServer();
        Debug.Log("[fix-net] WorldReadyStatic OnStartServer");
        StartCoroutine(InitializeStaticWorld());
    }

    private IEnumerator InitializeStaticWorld()
    {
        if (signaled)
            yield break;

        signaled = true;

        while (PlayerSpawnRegistry.I == null ||
               !PlayerSpawnRegistry.I.HasProvider)
        {
            yield return null;
        }

        var root = ServerCompositionRoot.I;

        root.Flow.NotifySceneLoaded();
        root.Flow.NotifyWorldPrepared();
        root.SetWorldType(WorldType.Static);

        Debug.Log("[fix-net] Static world RUNNING");

        root.Spawner.RespawnAllOnline();
    }
}
