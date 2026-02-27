using FishNet;
using FishNet.Managing.Scened;

public static class SceneTransitionService
{
    public const string WORLD_SCENE = "TestScene";

    public static void LoadWorldScene()
    {
        var nm = InstanceFinder.NetworkManager;
        if (nm == null || !nm.IsServer)
            return;

        var data = new SceneLoadData(WORLD_SCENE)
        {
            ReplaceScenes = ReplaceOption.All,
        };

        nm.SceneManager.LoadGlobalScenes(data);
    }
}