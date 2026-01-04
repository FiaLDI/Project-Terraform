using Features.Input;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneTransitionService
{
    public const string WORLD_SCENE = "TestScene";

    public static void LoadWorldScene()
    {
        SceneManager.LoadScene(WORLD_SCENE);
    }
    public static void RequestWorldScene()
    {
        var dispatcher = Object.FindFirstObjectByType<SceneTransitionDispatcher>();
        if (dispatcher == null)
        {
            UnityEngine.Debug.LogError("[SceneTransitionService] Dispatcher not found");
            return;
        }

        dispatcher.RequestWorldScene();
    }

}
