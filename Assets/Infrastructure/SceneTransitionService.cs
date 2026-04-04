using FishNet;
using FishNet.Managing.Scened;

public static class SceneTransitionService
{
    public const string NameWorldScene = "ProceduralWorld";
    public const string NameHubScene   = "NetHubScene";

    public static void LoadWorldScene()
    {
        var nm = InstanceFinder.NetworkManager;
        if (nm == null || !nm.IsServer)
            return;

        var data = new SceneLoadData(NameWorldScene)
        {
            ReplaceScenes = ReplaceOption.All,
        };

        nm.SceneManager.LoadGlobalScenes(data);
    }

    public static void LoadHubScene()
    {
        var nm = InstanceFinder.NetworkManager;
        if (nm == null || !nm.IsServer)
            return;

        var data = new SceneLoadData(NameHubScene)
        {
            ReplaceScenes = ReplaceOption.All,
        };

        nm.SceneManager.LoadGlobalScenes(data);
    }
}