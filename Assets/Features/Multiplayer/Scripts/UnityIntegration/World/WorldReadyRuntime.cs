using UnityEngine;
using FishNet;
using System.Collections;
using Features.Biomes.UnityIntegration;

public sealed class WorldReadyRuntime : MonoBehaviour
{
    private bool signaled;

    private void OnEnable()
    {
        RuntimeWorldGenerator.OnWorldReady += OnWorldReady;
    }

    private void OnDisable()
    {
        RuntimeWorldGenerator.OnWorldReady -= OnWorldReady;
    }

    private void OnWorldReady(int version)
    {
        if (!InstanceFinder.IsServer)
            return;

        if (signaled)
            return;

        signaled = true;

        StartCoroutine(WaitForSpawnProvidersAndRun());
    }

    private IEnumerator WaitForSpawnProvidersAndRun()
    {
        // 🔥 Ждём регистрацию spawn points
        while (PlayerSpawnRegistry.I == null ||
               !PlayerSpawnRegistry.I.HasProvider)
        {
            yield return null;
        }

        var root = ServerCompositionRoot.I;

        root.Flow.NotifySceneLoaded();
        root.Flow.NotifyWorldPrepared();

        // 🔥 Переспавниваем всех онлайн игроков
        root.Spawner.RespawnAllOnline();

        Debug.Log("[WorldReadyRuntime] Procedural world is RUNNING");
    }
}
