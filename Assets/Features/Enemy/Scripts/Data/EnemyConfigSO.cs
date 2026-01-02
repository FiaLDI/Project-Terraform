using Features.Enemy.Domain;
using UnityEngine;

namespace Features.Enemy.Data
{
    [CreateAssetMenu(menuName = "Enemies/Enemy Config")]
    public class EnemyConfigSO : ScriptableObject
    {
        [Header("ID / Info")]
        public string enemyId;
        public string displayName;
        public Sprite icon;

        [Header("AI")]
        public float patrolRadius = 10f;
        public float aggroRadius = 12f;
        public float attackRange = 2f;
        public float attackDamage = 10f;
        public float attackCooldown = 1.2f;

        [Header("Prefab Root (куда добавят компоненты)")]
        public GameObject prefab;

        [Header("Canvas prefab (HP-bar)")]
        public GameObject worldCanvasPrefab;

        [Header("Hitbox Multipliers")]
        public HitboxProfile[] hitboxes;


        [Header("Stats Preset")]
        public EnemyStatsPresetSO statsPreset;

        [Header("LOD Distances")]
        public float lod0Distance = 15f;
        public float lod1Distance = 40f;
        public float lod2Distance = 80f;

        [Header("Canvas Settings")]
        public float canvasHideDistance = 30f;

        [Header("GPU Instancing")]
        public bool useGPUInstancing = true;
        public float instancingDistance = 120f;
        public bool disableAnimatorInInstancing = true;
        public bool makeRigidbodyKinematicInInstancing = true;
    }
}
