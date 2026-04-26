using System.Collections;
using System.Collections.Generic;
using System;
using FishNet;
using FishNet.Managing;
using FishNet.Managing.Scened;
using FishNet.Object;
using UnityEngine;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;
using UnityLoadSceneMode = UnityEngine.SceneManagement.LoadSceneMode;
using UnityScene = UnityEngine.SceneManagement.Scene;
using Features.Game;

public static class SceneTransitionService
{
    public const string NameWorldScene = "ProceduralWorld";
    public const string NameHubScene   = "NetHubScene";
    public const string NameMainMenuScene   = "BootstrapScene";

    private sealed class Runner : MonoBehaviour
    {
    }

    private static Runner runner;
    private static NetworkManager currentNetworkManager;
    private static bool sceneQueueBusy;
    private static bool transitionRoutineRunning;
    private static bool returnToMainMenuRoutineRunning;
    private static string queuedSceneName;
    private static string activeSceneName;

    // ================= PUBLIC API =================

    public static void LoadMainMenuScene()
        => LoadScene(NameMainMenuScene);

    public static void LoadWorldScene()
        => LoadScene(NameWorldScene);

    public static void LoadHubScene()
        => LoadScene(NameHubScene);

    public static bool IsReturnToMainMenuInProgress => returnToMainMenuRoutineRunning;

    public static bool IsLocalSceneActive(string sceneName)
    {
        return string.Equals(
            UnitySceneManager.GetActiveScene().name,
            sceneName,
            StringComparison.Ordinal);
    }

    public static bool IsTransitionPendingFor(string sceneName)
    {
        return string.Equals(queuedSceneName, sceneName, StringComparison.Ordinal) ||
               string.Equals(activeSceneName, sceneName, StringComparison.Ordinal);
    }

    public static void ReturnAllPlayersToHub()
    {
        if (IsLocalSceneActive(NameHubScene) && !IsTransitionPendingFor(NameHubScene))
        {
            Debug.Log("[SceneTransition] Return to hub ignored: already in hub");
            return;
        }

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

        ServerWorldSession.ResetPendingWorldBootstrap();
        ServerWorldSession.ResetPendingQuestBootstrap();
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
        else if (sceneName == NameMainMenuScene)
            LoadingScreenService.Show("Main menu", "Synchronizing scene...");
        else
            LoadingScreenService.Show("Loading world", "Synchronizing scene...");

        Debug.Log($"[SceneTransition] Requested load '{sceneName}'");

        if (transitionRoutineRunning || runner == null)
            return;

        runner.StartCoroutine(ProcessQueuedTransition());
    }

    public static void ReturnToMainMenu()
    {
        if (returnToMainMenuRoutineRunning)
        {
            Debug.Log("[SceneTransition] Return to main menu already in progress");
            return;
        }

        EnsureRunnerExists();
        runner.StartCoroutine(ReturnToMainMenuRoutine());
    }

    public static void LoadMainMenuSceneLocal()
    {
        ResetTransitionState();
        BootstrapRoot.I?.ClearLocalPlayer();

        UnityScene activeScene = UnitySceneManager.GetActiveScene();
        if (string.Equals(activeScene.name, NameMainMenuScene, StringComparison.Ordinal))
            return;

        Debug.Log($"[SceneTransition] Loading local main menu scene '{NameMainMenuScene}'");
        UnitySceneManager.LoadScene(NameMainMenuScene, UnityLoadSceneMode.Single);
    }

    // ================= INTERNAL =================

    private static void EnsureInitialized(NetworkManager nm)
    {
        EnsureRunnerExists();

        if (currentNetworkManager == nm)
            return;

        UnsubscribeFromSceneEvents();

        currentNetworkManager = nm;
        currentNetworkManager.SceneManager.OnQueueStart += HandleQueueStart;
        currentNetworkManager.SceneManager.OnQueueEnd += HandleQueueEnd;
    }

    private static void EnsureRunnerExists()
    {
        if (runner != null)
            return;

        var go = new GameObject(nameof(SceneTransitionService));
        UnityEngine.Object.DontDestroyOnLoad(go);
        runner = go.AddComponent<Runner>();
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

    private static IEnumerator ReturnToMainMenuRoutine()
    {
        returnToMainMenuRoutineRunning = true;

        Debug.Log("[SceneTransition] Returning to main menu");
        LoadingScreenService.Show("Main menu", "Closing session...");

        ResetTransitionState();
        PreSceneCleanup();
        BootstrapRoot.I?.ClearLocalPlayer();
        ServerCompositionRoot.I?.ResetForMainMenu();

        var nm = InstanceFinder.NetworkManager;

        if (nm != null)
        {
            if (nm.ClientManager != null && nm.ClientManager.Started)
                nm.ClientManager.StopConnection();

            if (nm.ServerManager != null && nm.ServerManager.Started)
                nm.ServerManager.StopConnection(true);

            float deadline = Time.realtimeSinceStartup + 5f;

            while (Time.realtimeSinceStartup < deadline)
            {
                bool clientStarted = nm.ClientManager != null && nm.ClientManager.Started;
                bool serverStarted = nm.ServerManager != null && nm.ServerManager.Started;

                if (!clientStarted && !serverStarted)
                    break;

                yield return null;
            }
        }

        LoadMainMenuSceneLocal();
        returnToMainMenuRoutineRunning = false;
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

    private static void ResetTransitionState()
    {
        queuedSceneName = null;
        activeSceneName = null;
        sceneQueueBusy = false;
        transitionRoutineRunning = false;
        UnsubscribeFromSceneEvents();
        currentNetworkManager = null;
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
