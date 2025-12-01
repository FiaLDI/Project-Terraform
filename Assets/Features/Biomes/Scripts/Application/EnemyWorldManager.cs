using UnityEngine;
using System.Collections.Generic;
using Features.Enemies;

public class EnemyWorldManager : MonoBehaviour
{
    public static EnemyWorldManager Instance;

    [Header("Global Enemy Limits")]
    public int maxEnemiesInWorld = 150;

    private readonly List<EnemyDefinition> allEnemies = new();

    void Awake()
    {
        Instance = this;
    }

    public bool CanSpawn()
    {
        float scale = EnemyPerformanceManager.Instance != null
            ? EnemyPerformanceManager.Instance.EnemyCountScale
            : 1f;

        int softLimit = Mathf.RoundToInt(maxEnemiesInWorld * scale);
        return allEnemies.Count < softLimit;
    }

    public void Register(EnemyDefinition def)
    {
        if (def == null) return;
        if (!allEnemies.Contains(def))
            allEnemies.Add(def);
    }

    public void Unregister(EnemyDefinition def)
    {
        if (def == null) return;
        allEnemies.Remove(def);
    }

    public int GetCount() => allEnemies.Count;
}
