using FishNet.Object;
using FishNet.Managing.Scened;
using UnityEngine;
using FishNet.Connection;

public sealed class SceneTransitionDispatcher : NetworkBehaviour
{
    [SerializeField] private string worldSceneName = "WorldRuntime";

    public override void OnStartServer()
    {
        base.OnStartServer();
    }

    // ======================================================
    // CLIENT → SERVER
    // ======================================================

    public void RequestWorldScene()
    {
        if (!IsClient)
            return;

        RequestWorldSceneServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestWorldSceneServerRpc(NetworkConnection sender = null)
    {
        // 🔒 тут позже можно добавить проверки:
        // - только хост
        // - только лидер
        // - проверка состояния мира

        LoadWorldScene();
    }

    // ======================================================
    // SERVER
    // ======================================================

    private void LoadWorldScene()
    {
        var loadData = new SceneLoadData(worldSceneName)
        {
            ReplaceScenes = ReplaceOption.All
        };

        NetworkManager.SceneManager.LoadGlobalScenes(loadData);
    }
}
