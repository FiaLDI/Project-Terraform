using FishNet;
using FishNet.Managing.Scened;

public static class SceneTransitionService
{
    public const string NameWorldScene = "ProceduralWorld";
    public const string NameHubScene   = "NetHubScene";

    // ================= PUBLIC API =================

    public static void LoadWorldScene()
        => LoadScene(NameWorldScene);

    public static void LoadHubScene()
        => LoadScene(NameHubScene);

    public static void LoadScene(string sceneName)
    {
        var nm = InstanceFinder.NetworkManager;

        if (nm == null || !nm.IsServer)
            return;

        PreSceneCleanup();

        var data = new SceneLoadData(sceneName)
        {
            ReplaceScenes = ReplaceOption.All,
        };

        nm.SceneManager.LoadGlobalScenes(data);
    }

    // ================= INTERNAL =================

    private static void PreSceneCleanup()
    {
        UnityEngine.Debug.Log("[SceneTransition] Cleanup");
        
        PlayerRegistryECS.Clear();
        PlayerSpatialGrid.Clear();

        // 👉 сюда можно добавлять другие системы
        // ChunkManager?.ClearAll();
        // EnemyWorldManager?.Clear();
    }
}
