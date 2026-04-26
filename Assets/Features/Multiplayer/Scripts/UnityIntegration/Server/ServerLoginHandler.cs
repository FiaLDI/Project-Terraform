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

    public void HandleLogin(
        NetworkConnection conn,
        string persistentId,
        string characterId,
        string nickname,
        string classId,
        int level,
        int experience)
    {
        Debug.Log($"[SERVER] Received classId = {classId}");
        var session = sessions.HandleLogin(conn.ClientId, persistentId);

        session.SetCharacterData(characterId, nickname, classId, level, experience);
        Debug.Log($"[SESSION] Stored classId = {session.ClassId}");

        if (flow.CurrentState == ServerGameState.Running)
            spawner.HandleLoginSpawn(conn, session);
    }
}
