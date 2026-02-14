using FishNet;
using FishNet.Managing;
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
        Sessions.BindToFlow(Flow);

        Spawner = new SpawnService(
            net,
            Sessions,
            Flow,
            playerPrefab
        );

        // 🔥 ВАЖНО — внедряем зависимости
        loginHandler.Construct(
            Sessions,
            Spawner,
            Flow
        );

        Flow.OnStateChanged += OnServerStateChanged;

        Debug.Log("[CompositionRoot] Server systems initialized (DI complete)");
    }

    private void OnServerStateChanged(ServerGameState state)
    {
        if (state == ServerGameState.Starting)
        {
            Debug.Log("[CompositionRoot] Clearing sessions on server start");

            Sessions = new SessionManager();
            Sessions.BindToFlow(Flow);

            Spawner = new SpawnService(
                InstanceFinder.NetworkManager,
                Sessions,
                Flow,
                playerPrefab
            );

            loginHandler.Construct(
                Sessions,
                Spawner,
                Flow
            );
        }
    }

}
