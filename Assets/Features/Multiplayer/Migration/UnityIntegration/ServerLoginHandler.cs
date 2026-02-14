using UnityEngine;
using FishNet.Connection;
using Multiplayer.Application;
using Multiplayer.Domain;

public sealed class ServerLoginHandler : MonoBehaviour
{
    public static ServerLoginHandler I { get; private set; }

    private SessionManager sessions;
    private SpawnService spawner;
    private ServerGameFlow flow;

    private bool initialized;

    private void Awake()
    {
        if (I != null)
        {
            Destroy(gameObject);
            return;
        }

        I = this;
        DontDestroyOnLoad(gameObject);
    }

    // ✅ Вызывается CompositionRoot'ом
    public void Construct(
        SessionManager sessions,
        SpawnService spawner,
        ServerGameFlow flow)
    {
        this.sessions = sessions;
        this.spawner = spawner;
        this.flow = flow;

        initialized = true;

        Debug.Log("[ServerLoginHandler] Constructed via DI");
    }

    public void HandleLogin(NetworkConnection conn, string persistentId)
    {
        if (!initialized)
        {
            Debug.LogError("ServerLoginHandler not initialized!");
            return;
        }

        if (conn == null)
        {
            Debug.LogError("Login failed: conn is NULL");
            return;
        }

        Debug.Log($"LOGIN RECEIVED {conn.ClientId}");

        if (flow.CurrentState != ServerGameState.Running)
        {
            Debug.Log("Login rejected: world not ready");
            return;
        }

        var session = sessions.HandleLogin(conn.ClientId, persistentId);

        if (session == null)
        {
            Debug.Log("Session rejected");
            return;
        }

        spawner.HandleLoginSpawn(conn, session);
    }
}
