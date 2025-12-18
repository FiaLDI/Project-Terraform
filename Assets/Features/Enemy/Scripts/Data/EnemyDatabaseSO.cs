using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Features.Enemy.Data
{
    [CreateAssetMenu(menuName = "Enemies/EnemyDatabase")]
    public class EnemyDatabaseSO : ScriptableObject
    {
        public EnemyConfigSO[] enemies;

        private Dictionary<string, EnemyConfigSO> _map;

        private void OnValidate()
        {
            ValidateUniqueIds();
        }

        public void ValidateUniqueIds()
        {
            if (enemies == null) return;

            var duplicates = enemies
                .Where(e => e != null)
                .GroupBy(e => e.enemyId)
                .Where(g => g.Count() > 1)
                .ToList();

            if (duplicates.Count > 0)
            {
                foreach (var dup in duplicates)
                {
                    Debug.LogError(
                        $"[EnemyDatabase] Duplicate EnemyId found: {dup.Key}",
                        this
                    );
                }
            }
        }

        public EnemyConfigSO GetById(string id)
        {
            _map ??= enemies?
                .Where(e => e != null)
                .ToDictionary(e => e.enemyId, e => e);

            if (_map == null || !_map.TryGetValue(id, out var cfg))
                return null;

            return cfg;
        }

        public string[] GetAllIds()
        {
            if (enemies == null)
                return new string[0];

            return enemies
                .Where(e => e != null)
                .Select(e => e.enemyId)
                .ToArray();
        }
    }
}
