using FishNet;
using FishNet.Managing;
using FishNet.Transporting;
using FishNet.Managing.Scened;
using UnityEngine;


public sealed class BootstrapUI : MonoBehaviour
{
    [SerializeField] private string hubSceneName = "NetHubScene";

    private NetworkManager NM => InstanceFinder.NetworkManager;

    private void OnEnable()
    {
        // 🟢 ИСПРАВЛЕНИЕ: проверяем что NetworkManager доступен
        if (NM == null)
        {
            Debug.LogWarning("[BootstrapUI] NetworkManager not found yet, waiting...", this);
            return;
        }

        NM.ServerManager.OnServerConnectionState += OnServerState;
        Debug.Log("[BootstrapUI] Subscribed to server state changes", this);
    }

    private void OnDisable()
    {
        // 🟢 ИСПРАВЛЕНИЕ: безопасное отписание
        if (NM != null)
        {
            NM.ServerManager.OnServerConnectionState -= OnServerState;
            Debug.Log("[BootstrapUI] Unsubscribed from server state changes", this);
        }
    }

    // 🟢 ИСПРАВЛЕНИЕ: Start вместо OnEnable для инициализации при старте игры
    private void Start()
    {
        // Если не подписались в OnEnable (из-за null NM), подпишемся здесь
        if (NM != null && !HasSubscribed())
        {
            NM.ServerManager.OnServerConnectionState += OnServerState;
            Debug.Log("[BootstrapUI] Late subscription to server state changes", this);
        }
    }

    // =========================
    // HOST
    // =========================
    public void Host()
    {
        Debug.Log("[BootstrapUI] HOST pressed");

        // 🟢 ИСПРАВЛЕНИЕ: проверяем NetworkManager
        if (NM == null)
        {
            Debug.LogError("[BootstrapUI] NetworkManager is null! Cannot host.", this);
            return;
        }

        // 🟢 ИСПРАВЛЕНИЕ: проверяем что можем стартовать
        if (NM.ServerManager == null)
        {
            Debug.LogError("[BootstrapUI] ServerManager is null!", this);
            return;
        }

        if (NM.ClientManager == null)
        {
            Debug.LogError("[BootstrapUI] ClientManager is null!", this);
            return;
        }

        try
        {
            NM.ServerManager.StartConnection();
            NM.ClientManager.StartConnection();
            Debug.Log("[BootstrapUI] Host started successfully ✅", this);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[BootstrapUI] Error starting host: {ex.Message}", this);
        }
    }

    // =========================
    // CONNECT
    // =========================
    public void Connect()
    {
        Debug.Log("[BootstrapUI] CONNECT pressed");

        // 🟢 ИСПРАВЛЕНИЕ: проверяем NetworkManager
        if (NM == null)
        {
            Debug.LogError("[BootstrapUI] NetworkManager is null! Cannot connect.", this);
            return;
        }

        // 🟢 ИСПРАВЛЕНИЕ: проверяем ClientManager
        if (NM.ClientManager == null)
        {
            Debug.LogError("[BootstrapUI] ClientManager is null!", this);
            return;
        }

        try
        {
            NM.ClientManager.StartConnection();
            Debug.Log("[BootstrapUI] Client connection started ✅", this);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[BootstrapUI] Error starting connection: {ex.Message}", this);
        }
    }

    // =========================
    // SERVER READY CALLBACK
    // =========================
    private void OnServerState(ServerConnectionStateArgs args)
    {
        Debug.Log($"[BootstrapUI] Server state changed: {args.ConnectionState}", this);

        if (args.ConnectionState != LocalConnectionState.Started)
            return;

        Debug.Log("[BootstrapUI] Server started → loading Hub scene", this);

        LoadHubScene();
    }

    // =========================
    // SCENE LOAD
    // =========================
    private void LoadHubScene()
    {
        // 🟢 ИСПРАВЛЕНИЕ: проверяем NetworkManager
        if (NM == null)
        {
            Debug.LogError("[BootstrapUI] NetworkManager is null on scene load!", this);
            return;
        }

        if (!NM.IsServer)
        {
            Debug.LogWarning("[BootstrapUI] LoadHubScene called but not server", this);
            return;
        }

        // 🟢 ИСПРАВЛЕНИЕ: проверяем SceneManager
        if (NM.SceneManager == null)
        {
            Debug.LogError("[BootstrapUI] SceneManager is null!", this);
            return;
        }

        try
        {
            var data = new SceneLoadData(hubSceneName)
            {
                ReplaceScenes = ReplaceOption.All
            };

            NM.SceneManager.LoadGlobalScenes(data);
            Debug.Log($"[BootstrapUI] Loading scene: {hubSceneName} ✅", this);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[BootstrapUI] Error loading scene: {ex.Message}", this);
        }
    }

    private bool HasSubscribed()
    {
        // Проверяем что NM инициализирован
        return NM != null && NM.ServerManager != null;
    }
}
