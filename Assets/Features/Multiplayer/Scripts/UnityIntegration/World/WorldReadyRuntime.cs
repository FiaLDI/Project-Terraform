using UnityEngine;
using FishNet.Object;
using System.Collections;

public sealed class WorldReadyRuntime : NetworkBehaviour
{
    private bool initialized;
    private WorldProvider provider;

    public override void OnStartServer()
    {
        base.OnStartServer();
        StartCoroutine(ServerWaitFlow());
    }

    private IEnumerator ServerWaitFlow()
    {
        while (provider == null)
        {
            provider = FindObjectOfType<WorldProvider>();
            yield return null;
        }

        while (!provider.IsWorldReady.Value)
        {
            yield return null;
        }

        if (initialized)
            yield break;

        initialized = true;

        yield return InitializeWorld();
    }

    private IEnumerator InitializeWorld()
    {
        while (PlayerSpawnRegistry.I == null ||
               !PlayerSpawnRegistry.I.HasProvider)
        {
            yield return null;
        }

        var root = ServerCompositionRoot.I;

        root.Flow.NotifySceneLoaded();
        root.Flow.NotifyWorldPrepared();
        root.SetWorldType(WorldType.Dynamic);

        root.Spawner.RespawnAllOnline();

        if (NetworkTickSystem.I != null)
            NetworkTickSystem.I.Paused = false;
    }
}