using UnityEngine;

namespace Features.Enemy.Data
{
    [CreateAssetMenu(menuName = "Enemies/Enemy Config")]
    public class EnemyConfigSO : ScriptableObject
    {
        [Header("ID")]
        public string enemyId;
        public string displayName;

        [Header("Prefabs")]
        public GameObject prefab;

        [Header("Configs")]
        public EnemyAIConfigSO ai;
        public EnemyRenderConfigSO render;
        public EnemyCombatConfigSO combat;
        public EnemyStatsPresetSO stats;
    }
}
