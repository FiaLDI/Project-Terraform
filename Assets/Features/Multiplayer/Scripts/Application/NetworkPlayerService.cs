using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Transporting;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Features.Biomes.UnityIntegration;
using System.Linq;

public sealed class NetworkPlayerService : MonoBehaviour
{
    public static NetworkPlayerService I { get; private set; }

    [SerializeField] private NetworkObject playerPrefab;

    private readonly Dictionary<int, NetworkObject> players = new();
    private readonly HashSet<int> waitingForSpawn = new();

    private NetworkManager nm;

    // ===== STATE =====
    private bool serverStarted;
    private bool worldReady;
    private int currentWorldVersion = -1;
    private bool waitingForSpawnProvider;


    private enum SpawnContext
    {
        Hub,
        World
    }

    private SpawnContext spawnContext = SpawnContext.Hub;


    // ================= LIFECYCLE =================

    private void Awake()
    {
        Debug.Log("[NPS] OnEnable");

        if (I != null)
        {
            Destroy(gameObject);
            return;
        }

        I = this;
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        nm = InstanceFinder.NetworkManager;
    }

    private void OnEnable()
    {
        Debug.Log("[NPS] OnEnable");

        if (nm == null)
            return;

        nm.ServerManager.OnServerConnectionState += OnServerState;
        nm.ServerManager.OnRemoteConnectionState += OnRemoteConnectionState;

        RuntimeWorldGenerator.OnWorldReady += OnWorldReady;

        if (PlayerSpawnRegistry.I != null)
        {
            PlayerSpawnRegistry.I.OnProviderUnregistered += TrySpawnAll;
        }
    }

    private void OnDisable()
    {
        if (nm == null)
            return;

        nm.ServerManager.OnServerConnectionState -= OnServerState;
        nm.ServerManager.OnRemoteConnectionState -= OnRemoteConnectionState;

        RuntimeWorldGenerator.OnWorldReady -= OnWorldReady;

        if (PlayerSpawnRegistry.I != null)
        {
            PlayerSpawnRegistry.I.OnProviderUnregistered -= TrySpawnAll;
        }
    }

    // ================= STATE EVENTS =================

    private void OnServerState(ServerConnectionStateArgs args)
    {
        serverStarted = args.ConnectionState == LocalConnectionState.Started;
        Debug.Log(
            $"[NPS] ServerState={args.ConnectionState} " +
            $"serverStarted={serverStarted}"
        );
        TrySpawnAll();
    }

    private void OnWorldReady(int worldVersion)
    {
        worldReady = true;

        bool isNewWorld = worldVersion != currentWorldVersion;
        currentWorldVersion = worldVersion;

        if (players.Count > 0 && isNewWorld)
        {
            StartCoroutine(RepositionPlayers());
        }
        else
        {
            TrySpawnAll();
        }
    }

    private void OnRemoteConnectionState(
        NetworkConnection conn,
        RemoteConnectionStateArgs args)
    {
        Debug.Log(
            $"[NPS] RemoteConn id={conn?.ClientId} " +
            $"state={args.ConnectionState}"
        );
        if (!serverStarted || conn == null)
            return;

        if (args.ConnectionState == RemoteConnectionState.Started)
        {
            waitingForSpawn.Add(conn.ClientId);
            Debug.Log($"[NPS] Added {conn.ClientId} to waitingForSpawn");
            TrySpawnAll();
        }
        else if (args.ConnectionState == RemoteConnectionState.Stopped)
        {
            Despawn(conn);
        }
    }

    // ================= SPAWN STATE MACHINE =================

    private void TrySpawnAll()
    {
        Debug.Log(
            $"[FSM] TrySpawnAll ENTER | " +
            $"serverStarted={serverStarted} " +
            $"worldReady={worldReady} " +
            $"worldExists={RuntimeWorldGenerator.World != null} " +
            $"registry={(PlayerSpawnRegistry.I != null)} " +
            $"hasProvider={(PlayerSpawnRegistry.I != null && PlayerSpawnRegistry.I.HasProvider)} " +
            $"waitingCount={waitingForSpawn.Count} " +
            $"players={players.Count}"
        );

        if (!serverStarted)
        {
            Debug.Log("[FSM] EXIT: server not started");
            return;
        }

        if (RuntimeWorldGenerator.World != null && !worldReady)
        {
            Debug.Log("[FSM] EXIT: world exists but not ready");
            return;
        }

        if (PlayerSpawnRegistry.I == null)
        {
            Debug.Log("[FSM] EXIT: no spawn registry");
            return;
        }

        if (!PlayerSpawnRegistry.I.HasProvider)
        {
            Debug.Log("[FSM] No provider → waiting");
            WaitForSpawnProvider();
            return;
        }

        StopWaitingForSpawnProvider();

        if (waitingForSpawn.Count == 0)
        {
            Debug.Log("[FSM] EXIT: no clients waiting");
            return;
        }

        foreach (int clientId in waitingForSpawn.ToArray())
        {
            Debug.Log($"[FSM] Processing clientId={clientId}");

            bool hasConn = nm.ServerManager.Clients.TryGetValue(clientId, out var conn);
            Debug.Log($"[FSM] Clients.TryGetValue({clientId}) = {hasConn}");

            if (!hasConn || conn == null)
            {
                Debug.Log($"[FSM] Client {clientId} not ready yet");
                continue;
            }

            bool alreadySpawned = players.ContainsKey(clientId);
            Debug.Log($"[FSM] alreadySpawned={alreadySpawned}");

            if (alreadySpawned)
            {
                waitingForSpawn.Remove(clientId);
                continue;
            }

            bool gotProvider = PlayerSpawnRegistry.I.TryGetRandom(out var provider);
            Debug.Log($"[FSM] TryGetRandom provider = {gotProvider}");

            if (!gotProvider || provider == null)
            {
                Debug.Log("[FSM] Provider invalid");
                continue;
            }

            Debug.Log($"[FSM] START SpawnPlayer coroutine for client {clientId}");

            waitingForSpawn.Remove(clientId);
            Debug.Log(
                $"[FSM] About to StartCoroutine | " +
                $"active={gameObject.activeInHierarchy} " +
                $"enabled={enabled}"
            );

            StartCoroutine(SpawnPlayerWrapper(conn, provider));
        }

        Debug.Log("[FSM] TrySpawnAll EXIT");
    }

    private IEnumerator SpawnPlayerWrapper(
    NetworkConnection conn,
    IPlayerSpawnProvider provider)
    {
        Debug.Log("[WRAPPER] Coroutine wrapper START");

        yield return SpawnPlayer(conn, provider);

        Debug.Log("[WRAPPER] Coroutine wrapper END");
    }


    private void WaitForSpawnProvider()
    {
        if (waitingForSpawnProvider)
            return;

        waitingForSpawnProvider = true;

        Debug.Log("[NPS] Waiting for spawn provider...");

        PlayerSpawnRegistry.I.OnProviderRegistered += OnSpawnProviderAvailable;
    }

    private void StopWaitingForSpawnProvider()
    {
        if (!waitingForSpawnProvider)
            return;

        waitingForSpawnProvider = false;

        PlayerSpawnRegistry.I.OnProviderRegistered -= OnSpawnProviderAvailable;

        Debug.Log("[NPS] Spawn provider found, stop waiting");
    }

    private void OnSpawnProviderAvailable()
    {
        Debug.Log("[NPS] Spawn provider registered");

        TrySpawnAll();
    }


    private IEnumerator SpawnPlayer(
    NetworkConnection conn,
    IPlayerSpawnProvider provider)
    {
        Debug.Log($"[SPAWN] Coroutine START client={conn.ClientId}");

        // подождать ОДИН кадр, не физику
        yield return null;

        if (players.ContainsKey(conn.ClientId))
        {
            Debug.Log("[SPAWN] Abort: already spawned");
            yield break;
        }

        if (!provider.TryGetSpawnPoint(out var pos, out var rot))
        {
            Debug.Log("[SPAWN] Abort: no spawn point");
            yield break;
        }

        pos = SnapToGround(pos);

        var player = Instantiate(playerPrefab, pos, rot);
        nm.ServerManager.Spawn(player, conn);

        players[conn.ClientId] = player;

        Debug.Log($"[SPAWN] Player spawned client={conn.ClientId}");
    }


    // ================= HOT RELOAD =================

    private IEnumerator RepositionPlayers()
    {
        // ждём физику нового мира
        yield return new WaitForFixedUpdate();

        Debug.Log("[PlayerService] Repositioning players after world reload");

        foreach (var pair in players)
        {
            var player = pair.Value;
            if (player == null)
                continue;

            if (!PlayerSpawnRegistry.I.TryGetRandom(out var provider))
                continue;

            if (!provider.TryGetSpawnPoint(out var pos, out var rot))
                continue;

            yield return WaitForGround(pos);

            player.transform.SetPositionAndRotation(
                SnapToGround(pos),
                rot
            );
        }
    }

    private IEnumerator WaitForGround(
    Vector3 origin,
    float timeout = 5f)
    {
        float t = 0f;

        while (t < timeout)
        {
            if (Physics.Raycast(
                origin + Vector3.up * 30f,
                Vector3.down,
                out _,
                200f,
                ~0,
                QueryTriggerInteraction.Ignore))
            {
                yield break; // земля есть
            }

            t += Time.deltaTime;
            yield return null;
        }

        Debug.LogWarning("[Spawn] Ground not found, fallback");
    }


    // ================= UTILS =================

    private Vector3 SnapToGround(Vector3 pos)
    {
        if (Physics.Raycast(
            pos + Vector3.up * 10f,
            Vector3.down,
            out var hit,
            50f,
            ~0,
            QueryTriggerInteraction.Ignore))
        {
            return hit.point + Vector3.up * 0.2f;
        }

        return pos;
    }

    // ================= DESPAWN =================

    private void Despawn(NetworkConnection conn)
    {
        if (conn == null)
            return;

        waitingForSpawn.Remove(conn.ClientId);

        if (players.TryGetValue(conn.ClientId, out var player))
        {
            if (player != null && player.IsSpawned)
                player.Despawn();

            players.Remove(conn.ClientId);
        }
    }
    public void SetSpawnContextHub()
    {
        spawnContext = SpawnContext.Hub;
        worldReady = false; // на всякий
        TrySpawnAll();
    }

    public void SetSpawnContextWorld()
    {
        spawnContext = SpawnContext.World;
        worldReady = true;
        TrySpawnAll();
    }
}
