using System.Collections.Generic;
using UnityEngine;

public sealed class PlayerSpawnRegistry : MonoBehaviour
{
    public static PlayerSpawnRegistry I { get; private set; }

    private readonly List<IPlayerSpawnProvider> providers = new();
    private readonly Dictionary<int, PlayerSpawnOverride> playerOverrides = new();

    public event System.Action OnProviderRegistered;
    public event System.Action OnProviderUnregistered;

    private void Awake()
    {
        if (I != null)
        {
            Destroy(gameObject);
            return;
        }
        transform.SetParent(null);
        I = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Register(IPlayerSpawnProvider provider)
    {
        if (provider == null)
            return;

        CleanupDead();

        if (providers.Contains(provider))
            return;

        providers.Add(provider);
        Debug.Log($"[SpawnRegistry] Registered {provider}", provider as Object);

        OnProviderRegistered?.Invoke();
    }

    public void Unregister(IPlayerSpawnProvider provider)
    {
        if (provider == null)
            return;

        CleanupDead();

        if (providers.Remove(provider))
        {
            Debug.Log($"[SpawnRegistry] Unregistered {provider}", provider as Object);
            OnProviderUnregistered?.Invoke();
        }
    }

    public bool HasProvider
    {
        get
        {
            CleanupDead();
            return providers.Count > 0;
        }
    }

    public bool TryGetRandom(out IPlayerSpawnProvider provider)
    {
        CleanupDead();

        if (providers.Count == 0)
        {
            provider = null;
            return false;
        }

        provider = providers[Random.Range(0, providers.Count)];
        return provider != null;
    }

    private void CleanupDead()
    {
        providers.RemoveAll(p => p == null || (p is Object o && o == null));
    }

    public bool TryGetSpawnPoint(out Vector3 pos, out Quaternion rot)
    {
        pos = default;
        rot = default;

        if (!TryGetRandom(out var provider))
            return false;

        return provider.TryGetSpawnPoint(out pos, out rot);
    }

    public void SetPlayerSpawnPoint(int clientId, Vector3 pos, Quaternion rot)
    {
        playerOverrides[clientId] = new PlayerSpawnOverride(pos, rot);
        Debug.Log($"[SpawnRegistry] Player {clientId} checkpoint set at {pos}");
    }

    public void ClearPlayerSpawnPoint(int clientId)
    {
        playerOverrides.Remove(clientId);
    }

    public void ClearPlayerSpawnPoints()
    {
        playerOverrides.Clear();
    }

    public bool TryGetSpawnPoint(int clientId, out Vector3 pos, out Quaternion rot)
    {
        if (playerOverrides.TryGetValue(clientId, out var spawn))
        {
            pos = spawn.Position;
            rot = spawn.Rotation;
            return true;
        }

        return TryGetSpawnPoint(out pos, out rot);
    }

    private readonly struct PlayerSpawnOverride
    {
        public readonly Vector3 Position;
        public readonly Quaternion Rotation;

        public PlayerSpawnOverride(Vector3 position, Quaternion rotation)
        {
            Position = position;
            Rotation = rotation;
        }
    }
}
