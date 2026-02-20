using UnityEngine;
using FishNet.Object;
using System.Collections;
using Features.Biomes.UnityIntegration;

public sealed class WorldReadyRuntime : NetworkBehaviour
{
    private bool signaled;

    public override void OnStartServer()
    {
        base.OnStartServer();

        RuntimeWorldGenerator.OnWorldReady += OnWorldReady;
    }

    public override void OnStopServer()
    {
        base.OnStopServer();

        RuntimeWorldGenerator.OnWorldReady -= OnWorldReady;
    }

    private void OnWorldReady(int version)
    {
        if (signaled)
            return;

        signaled = true;

        StartCoroutine(InitializeWorld());
    }

    private IEnumerator InitializeWorld()
    {
        // Ждём регистрацию spawn providers
        while (PlayerSpawnRegistry.I == null ||
               !PlayerSpawnRegistry.I.HasProvider)
        {
            yield return null;
        }

        var root = ServerCompositionRoot.I;

        root.Flow.NotifySceneLoaded();
        root.Flow.NotifyWorldPrepared();

        Debug.Log("[fix-net] Server world RUNNING");

        root.Spawner.RespawnAllOnline();

        if (NetworkTickSystem.I != null)
            NetworkTickSystem.I.Paused = false;
    }
}
