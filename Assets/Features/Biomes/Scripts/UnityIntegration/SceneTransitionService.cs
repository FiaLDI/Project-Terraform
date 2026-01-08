using Features.Input;
using FishNet;
using FishNet.Managing.Scened;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneTransitionService
{
    public const string WORLD_SCENE = "TestScene";

    public static void RequestWorldScene()
    {
        var nm = InstanceFinder.NetworkManager;
        if (nm == null)
        {
            Debug.LogError("[SceneTransition] NetworkManager not found");
            return;
        }

        // 🔴 КРИТИЧНО: сцену грузит ТОЛЬКО сервер
        if (!nm.IsServer)
        {
            Debug.LogWarning("[SceneTransition] Only server can load world scene");
            return;
        }

        var data = new SceneLoadData(WORLD_SCENE)
        {
            ReplaceScenes = ReplaceOption.All
        };

        nm.SceneManager.LoadGlobalScenes(data);
    }

}
