using UnityEngine;
using FishNet;
using FishNet.Managing;
using FishNet.Transporting;

public sealed class ScenePlayerSpawnPoint : MonoBehaviour, IPlayerSpawnProvider
{
    private NetworkManager nm;
    private bool registered;

    private void Awake()
    {
        nm = InstanceFinder.NetworkManager;

        // ❗ ВСЕГДА подписываемся
        if (nm != null)
            nm.ServerManager.OnServerConnectionState += OnServerState;
    }

    private void Start()
    {
        // 🔑 ВАЖНО: пробуем зарегистрироваться ПОСЛЕ Awake всей сцены
        TryRegister();
    }

    private void OnDestroy()
    {
        if (nm != null)
            nm.ServerManager.OnServerConnectionState -= OnServerState;

        Unregister();
    }

    private void OnServerState(ServerConnectionStateArgs args)
    {
        if (args.ConnectionState == LocalConnectionState.Started)
            Register();
        else if (args.ConnectionState == LocalConnectionState.Stopped)
            Unregister();
    }

    private void TryRegister()
    {
        if (nm != null && nm.IsServer)
            Register();
    }

    private void Register()
    {
        if (registered)
            return;

        registered = true;
        PlayerSpawnRegistry.I?.Register(this);

        Debug.Log($"[SpawnPoint] Registered {name}");
    }

    private void Unregister()
    {
        if (!registered)
            return;

        registered = false;
        PlayerSpawnRegistry.I?.Unregister(this);
    }

    public bool TryGetSpawnPoint(out Vector3 pos, out Quaternion rot)
    {
        pos = transform.position;
        rot = transform.rotation;
        return true;
    }
}
