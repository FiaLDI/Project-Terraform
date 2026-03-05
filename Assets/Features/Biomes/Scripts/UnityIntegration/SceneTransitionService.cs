using FishNet;
using FishNet.Managing.Scened;

public static class SceneTransitionService
{
    public const string WORLD_SCENE = "TestScene";
    public const string HUB_SCENE   = "NetHubScene";

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

    public static void LoadHubScene() // 👈 новый метод
    {
        var nm = InstanceFinder.NetworkManager;
        if (nm == null || !nm.IsServer)
            return;

        var data = new SceneLoadData(HUB_SCENE)
        {
            ReplaceScenes = ReplaceOption.All,
        };

        nm.SceneManager.LoadGlobalScenes(data);
    }
}