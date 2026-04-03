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

        [Header("Prefab Root")]
        public GameObject prefab;

        [Header("LOD Prefabs")]
        public GameObject lod0Prefab;
        public GameObject lod1Prefab;
        public GameObject lod2Prefab;

        [Header("Canvas prefab")]
        public GameObject worldCanvasPrefab;

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

        [Header("Attack Tuning")]
        public float attackEnterOffset = 0.5f;
        public float attackExitOffset = 1.0f;
        public float stopDistanceMultiplier = 0.7f;

        [Header("Vision")]
        [Range(10,180)] public float visionAngle = 120f;
        public float visionRange = 12f;
        public bool requireLineOfSight = true;

        [Header("Animation")]
        public RuntimeAnimatorController animatorController;

        [Header("Physics")]
        public LayerMask obstacleMask;

        // =========================================================
        // 🔥 NEW: STEERING SETTINGS
        // =========================================================

        [Header("Steering Weights")]
        public float seekWeight = 1.0f;
        public float avoidWeight = 2.2f;
        public float separationWeight = 1.5f;
        public float orbitWeight = 0.8f;

        [Header("Steering Distances")]
        public float avoidDistance = 1.5f;
        public float sideAvoidDistance = 1.2f;
        public float separationRadius = 2.0f;

        [Header("Movement Feel")]
        public float rotationSpeed = 8f;
        public float orbitStrength = 0.6f;

        [Header("Behavior")]
        public bool enableSeparation = true;
        public bool enableAvoidance = true;
        public bool enableOrbit = true;
    }
}
