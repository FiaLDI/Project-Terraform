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
            return;

        if (!nm.IsServer)
            return;

        NetworkTickSystem.I.Paused = true;

        var data = new SceneLoadData(WORLD_SCENE)
        {
            ReplaceScenes = ReplaceOption.All
        };

        nm.SceneManager.LoadGlobalScenes(data);
    }


}
