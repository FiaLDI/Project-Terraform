using UnityEngine;
using Biomes.Data;
using Biomes.Application;
using Biomes.UnityIntegration;

public class EnemyAutoUnregister : MonoBehaviour
{
    [HideInInspector] public BiomeConfig biome;
    [HideInInspector] public EnemyInstanceTracker tracker;

    private bool _unregistered = false;

    private void OnDisable()
    {
        TryUnregister();
    }

    private void OnDestroy()
    {
        TryUnregister();
    }

    private void TryUnregister()
    {
        if (_unregistered) return;
        if (tracker == null) return;

        _unregistered = true;

        EnemyWorldManager.Instance?.Unregister(tracker);

        if (biome != null)
            EnemyBiomeCounter.Unregister(biome, tracker);
    }
}
