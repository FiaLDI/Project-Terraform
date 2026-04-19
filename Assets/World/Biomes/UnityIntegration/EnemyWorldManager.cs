using UnityEngine;
using System.Collections.Generic;
using Biomes.Application;

namespace Biomes.UnityIntegration
{
    public class EnemyWorldManager : MonoBehaviour
    {
        public static EnemyWorldManager Instance;

        [Header("Global Enemy Limit")]
        public int maxEnemiesInWorld = 150;

        private readonly List<EnemyInstanceTracker> enemies = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public bool CanSpawn(int configuredLimit = -1)
        {
            CleanupDead();

            float scale = EnemyPerformanceManager.Instance != null
                ? EnemyPerformanceManager.Instance.EnemyCountScale
                : 1f;

            int baseLimit = configuredLimit > 0
                ? Mathf.Min(configuredLimit, maxEnemiesInWorld)
                : maxEnemiesInWorld;

            int softLimit = Mathf.Max(1, Mathf.RoundToInt(baseLimit * scale));
            return enemies.Count < softLimit;
        }

        public void Register(EnemyInstanceTracker inst)
        {
            if (inst == null) return;
            CleanupDead();

            if (!enemies.Contains(inst))
                enemies.Add(inst);
        }

        public void Unregister(EnemyInstanceTracker inst)
        {
            if (inst == null) return;
            enemies.Remove(inst);
        }

        public int GetCount()
        {
            CleanupDead();
            return enemies.Count;
        }

        private void CleanupDead()
        {
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                var enemy = enemies[i];
                if (enemy == null || !enemy.isActiveAndEnabled)
                    enemies.RemoveAt(i);
            }
        }

        public void ClearRuntimeState()
        {
            enemies.Clear();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;

            enemies.Clear();
        }
    }
}
