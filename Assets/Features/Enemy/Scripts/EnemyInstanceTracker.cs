// EnemyInstanceTracker.cs
using System.Collections.Generic;
using UnityEngine;
using Features.Enemy.Data;

public class EnemyInstanceTracker : MonoBehaviour
{
    public static readonly List<EnemyInstanceTracker> All = new();

    public EnemyConfigSO config;

    private void OnEnable()
    {
        if (!All.Contains(this))
            All.Add(this);
    }

    private void OnDisable()
    {
        All.Remove(this);
    }
}
