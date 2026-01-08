using System.Collections.Generic;
using UnityEngine;

public sealed class PlayerSpawnRegistry : MonoBehaviour
{
    public static PlayerSpawnRegistry I { get; private set; }

    private readonly List<IPlayerSpawnProvider> providers = new();

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
}
