using UnityEngine;
using System.Collections.Generic;
using Features.Enemies;

public class EnemyWorldManager : MonoBehaviour
{
    public static EnemyWorldManager Instance;

    [Header("Global Enemy Limit")]
    public int maxEnemiesInWorld = 150;

    private readonly List<EnemyInstanceTracker> enemies = new();

    void Awake() => Instance = this;

    public bool CanSpawn()
    {
        float scale = EnemyPerformanceManager.Instance != null
            ? EnemyPerformanceManager.Instance.EnemyCountScale
            : 1f;

        int softLimit = Mathf.RoundToInt(maxEnemiesInWorld * scale);
        return enemies.Count < softLimit;
    }

    public void Register(EnemyInstanceTracker inst)
    {
        if (inst == null) return;
        if (!enemies.Contains(inst))
            enemies.Add(inst);
    }

    public void Unregister(EnemyInstanceTracker inst)
    {
        if (inst == null) return;
        enemies.Remove(inst);
    }

    public int GetCount() => enemies.Count;
}