using UnityEngine;

namespace Features.Enemy.Data
{
    [CreateAssetMenu(menuName = "Enemies/Stats Preset")]
    public class EnemyStatsPresetSO : ScriptableObject
    {
        [System.Serializable]
        public class HealthBlock
        {
            public float baseHp = 100f;
            public float baseRegen = 0f;
        }

        [System.Serializable]
        public class CombatBlock
        {
            public float baseDamageMultiplier = 1f;
        }

        [Header("Health")]
        public HealthBlock health = new();

        [Header("Combat")]
        public CombatBlock combat = new();
    }
}
