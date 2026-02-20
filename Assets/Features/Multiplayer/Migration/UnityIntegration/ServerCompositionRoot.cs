using FishNet;
using FishNet.Managing;
using FishNet.Managing.Scened;
using FishNet.Object;
using Multiplayer.Application;
using Multiplayer.Domain;
using UnityEngine;

public sealed class ServerCompositionRoot : MonoBehaviour
{
    public static ServerCompositionRoot I { get; private set; }

    public SessionManager Sessions { get; private set; }
    public SpawnService Spawner { get; private set; }
    public ServerGameFlow Flow { get; private set; }

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

    private void Initialize()
    {
        var net = InstanceFinder.NetworkManager;

        Flow = new ServerGameFlow();

        Sessions = new SessionManager();

        Spawner = new SpawnService(net, Sessions, Flow, playerPrefab);

        loginHandler.Construct(Sessions, Spawner, Flow);

        Debug.Log("[CompositionRoot] Server systems initialized");
    }
}
