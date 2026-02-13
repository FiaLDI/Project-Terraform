
using FishNet.Connection;
using Multiplayer.Application;
using UnityEngine;

public sealed class ServerLoginHandler : MonoBehaviour
{
    public static ServerLoginHandler I;

    private SessionManager sessions;
    private SpawnService spawner;

    private void Awake()
    {
        I = this;
    }

    public void Init(SessionManager sm, SpawnService ss)
    {
        sessions = sm;
        spawner = ss;
    }

    public void HandleLogin(NetworkConnection conn, string pid)
    {
        var session = sessions.HandleLogin(conn.ClientId, pid);

        if (session == null)
            return;

        spawner.HandleLoginSpawn(conn, session);
    }
}
