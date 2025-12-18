using UnityEngine;
using Features.Biomes.Domain;

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

        // Убираем из глобального учёта
        EnemyWorldManager.Instance?.Unregister(tracker);

        // Убираем из биома
        if (biome != null)
            EnemyBiomeCounter.Unregister(biome, tracker);
    }
}
