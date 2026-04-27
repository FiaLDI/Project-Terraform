using FishNet;
using FishNet.Object;
using Multiplayer.Application;
using UnityEngine;
public enum WorldType
{
    Static,
    Dynamic
}

public sealed class ServerCompositionRoot : MonoBehaviour
{
    public static ServerCompositionRoot I { get; private set; }

    public SessionManager Sessions { get; private set; }
    public SpawnService Spawner { get; private set; }
    public ServerGameFlow Flow { get; private set; }
    public WorldType CurrentWorldType { get; private set; } = WorldType.Static;

    [Header("References")]
    [SerializeField] private NetworkObject playerPrefab;
    [SerializeField] private ServerLoginHandler loginHandler;

    private void Awake()
    {
        if (I != null)
        {
            Destroy(gameObject);
            return;
        }

        I = this;
        DontDestroyOnLoad(gameObject);

        Initialize();
    }

    public void SetWorldType(WorldType type)
    {
        CurrentWorldType = type;

        Debug.Log($"[World] Type set to: {type}");
    }

    private void Initialize()
    {
        var net = InstanceFinder.NetworkManager;

        Flow = new ServerGameFlow();

        Sessions = new SessionManager();

        Spawner = new SpawnService(net, Sessions, Flow, playerPrefab);

        loginHandler.Construct(Sessions, Spawner, Flow);

        Debug.Log("[CompositionRoot] Server systems initialized");
    }

    public void ResetForMainMenu()
    {
        Flow?.Shutdown();
        Sessions?.ResetAll();
        CurrentWorldType = WorldType.Static;

        Debug.Log("[CompositionRoot] Reset for main menu");
    }
}
