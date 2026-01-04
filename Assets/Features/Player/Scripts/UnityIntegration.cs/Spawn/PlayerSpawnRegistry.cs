using System.Collections.Generic;
using UnityEngine;

public sealed class PlayerSpawnRegistry : MonoBehaviour
{
    public static PlayerSpawnRegistry I { get; private set; }

    private readonly List<IPlayerSpawnProvider> providers = new();

    // ================= EVENTS =================
    public event System.Action OnProviderRegistered;
    public event System.Action OnProviderUnregistered;

    // ================= LIFECYCLE =================

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

    // ================= API =================

    public void Register(IPlayerSpawnProvider provider)
    {
        if (provider == null || providers.Contains(provider))
            return;

        providers.Add(provider);
        Debug.Log($"[SpawnRegistry] Registered {provider}", provider as Object);

        OnProviderRegistered?.Invoke();
    }

    public void Unregister(IPlayerSpawnProvider provider)
    {
        if (provider == null)
            return;

        if (providers.Remove(provider))
        {
            Debug.Log($"[SpawnRegistry] Unregistered {provider}", provider as Object);
            OnProviderUnregistered?.Invoke();
        }
    }

    public bool HasProvider => providers.Count > 0;

    public bool TryGetRandom(out IPlayerSpawnProvider provider)
    {
        if (providers.Count == 0)
        {
            provider = null;
            return false;
        }

        provider = providers[Random.Range(0, providers.Count)];
        return true;
    }
}
