using System.Collections;
using System.Collections.Generic;
using System;
using FishNet;
using FishNet.Managing;
using FishNet.Managing.Scened;
using FishNet.Object;
using UnityEngine;

public static class SceneTransitionService
{
    public const string NameWorldScene = "ProceduralWorld";
    public const string NameHubScene   = "NetHubScene";

    private sealed class Runner : MonoBehaviour
    {
    }

    private static Runner runner;
    private static NetworkManager currentNetworkManager;
    private static bool sceneQueueBusy;
    private static bool transitionRoutineRunning;
    private static string queuedSceneName;
    private static string activeSceneName;

    // ================= PUBLIC API =================

    public static void LoadWorldScene()
        => LoadScene(NameWorldScene);

    public static void LoadHubScene()
        => LoadScene(NameHubScene);

    public static bool IsTransitionPendingFor(string sceneName)
    {
        return string.Equals(queuedSceneName, sceneName, StringComparison.Ordinal) ||
               string.Equals(activeSceneName, sceneName, StringComparison.Ordinal);
    }

    public static void ReturnAllPlayersToHub()
    {
        var sessions = ServerCompositionRoot.I?.Sessions;

        if (sessions != null)
        {
            foreach (var onlineSession in sessions.GetOnlineSessions())
            {
                onlineSession.SetPendingWorldQuestBootstrap(null, null);
                onlineSession.PlayerObject?.GetComponent<PlayerSessionNetwork>()?.ShowHubLoadingObserversRpc();
                onlineSession.PlayerObject?.GetComponent<PlayerQuestComponent>()?.ClearAll();
            }
        }

        ServerWorldSession.PendingSeed = 0;
        ServerWorldSession.PendingWorldConfigId = string.Empty;
        ServerWorldSession.PendingQuestIds.Clear();
        ServerWorldSession.PendingChainIds.Clear();
        PlayerSpawnRegistry.I?.ClearPlayerSpawnPoints();

        LoadHubScene();
    }

    public static void LoadScene(string sceneName)
    {
        var nm = InstanceFinder.NetworkManager;

        if (nm == null || !nm.IsServerStarted)
            return;

        EnsureInitialized(nm);

        if (IsTransitionPendingFor(sceneName))
        {
            Debug.Log($"[SceneTransition] Duplicate load '{sceneName}' ignored");
            return;
        }

        queuedSceneName = sceneName;

        if (sceneName == NameHubScene)
            LoadingScreenService.ShowHub("Returning players to hub...");
        else
            LoadingScreenService.Show("Loading world", "Synchronizing scene...");

        Debug.Log($"[SceneTransition] Requested load '{sceneName}'");

        if (transitionRoutineRunning || runner == null)
            return;

        runner.StartCoroutine(ProcessQueuedTransition());
    }

    // ================= INTERNAL =================

    private static void EnsureInitialized(NetworkManager nm)
    {
        if (runner == null)
        {
            var go = new GameObject(nameof(SceneTransitionService));
            UnityEngine.Object.DontDestroyOnLoad(go);
            runner = go.AddComponent<Runner>();
        }

        if (currentNetworkManager == nm)
            return;

        UnsubscribeFromSceneEvents();

        currentNetworkManager = nm;
        currentNetworkManager.SceneManager.OnQueueStart += HandleQueueStart;
        currentNetworkManager.SceneManager.OnQueueEnd += HandleQueueEnd;
    }

    private static IEnumerator ProcessQueuedTransition()
    {
        if (transitionRoutineRunning)
            yield break;

        transitionRoutineRunning = true;

        while (!string.IsNullOrWhiteSpace(queuedSceneName))
        {
            string nextSceneName = queuedSceneName;
            queuedSceneName = null;

            // Defer out of the RPC/transport callback stack before touching FishNet scene loading.
            yield return null;

            while (sceneQueueBusy)
                yield return null;

            if (currentNetworkManager == null || !currentNetworkManager.IsServerStarted)
                break;

            activeSceneName = nextSceneName;
            PreSceneCleanup();

            NetworkObject[] movedObjects = CollectMovedNetworkObjects(currentNetworkManager);

            var data = new SceneLoadData(nextSceneName)
            {
                ReplaceScenes = ReplaceOption.All,
                MovedNetworkObjects = movedObjects,
            };

            sceneQueueBusy = true;

            Debug.Log($"[SceneTransition] Loading '{nextSceneName}' | moved={movedObjects.Length}");

            currentNetworkManager.SceneManager.LoadGlobalScenes(data);

            while (sceneQueueBusy)
                yield return null;

            activeSceneName = null;
        }

        transitionRoutineRunning = false;

        if (!string.IsNullOrWhiteSpace(queuedSceneName) && runner != null)
            runner.StartCoroutine(ProcessQueuedTransition());
    }

    private static NetworkObject[] CollectMovedNetworkObjects(NetworkManager nm)
    {
        var result = new List<NetworkObject>();
        var root = ServerCompositionRoot.I;

        if (root?.Sessions != null)
        {
            foreach (var session in root.Sessions.GetOnlineSessions())
                AddIfValid(result, session.PlayerObject);
        }

        foreach (NetworkObject spawned in nm.ServerManager.Objects.Spawned.Values)
        {
            if (spawned == null || spawned.IsGlobal)
                continue;

            if (spawned.GetComponent<ConnectionObject>() != null)
                AddIfValid(result, spawned);
        }

        return result.ToArray();
    }

    private static void AddIfValid(List<NetworkObject> result, NetworkObject networkObject)
    {
        if (networkObject == null || result.Contains(networkObject))
            return;

        result.Add(networkObject);
    }

    private static void HandleQueueStart()
    {
        sceneQueueBusy = true;
    }

    private static void HandleQueueEnd()
    {
        sceneQueueBusy = false;
    }

    private static void UnsubscribeFromSceneEvents()
    {
        if (currentNetworkManager == null)
            return;

        currentNetworkManager.SceneManager.OnQueueStart -= HandleQueueStart;
        currentNetworkManager.SceneManager.OnQueueEnd -= HandleQueueEnd;
    }

    private static void PreSceneCleanup()
    {
        Debug.Log("[SceneTransition] Cleanup");
        
        PlayerRegistryECS.Clear();
        PlayerSpatialGrid.Clear();
        PlayerSpawnRegistry.I?.ClearPlayerSpawnPoints();

        // 👉 сюда можно добавлять другие системы
        // ChunkManager?.ClearAll();
        // EnemyWorldManager?.Clear();
    }
}
