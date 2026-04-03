using UnityEngine;

namespace Features.Enemy.Data
{
    [CreateAssetMenu(menuName = "Enemies/AI Config")]
    public class EnemyAIConfigSO : ScriptableObject
    {
        [Header("Aggro")]
        public float aggroRadius = 12f;
        public float loseAggroRadius = 18f;

        [Header("Movement")]
        public float moveSpeed = 3f;
        public float rotationSpeed = 8f;

        [Header("Vision")]
        [Range(10, 180)]
        public float visionAngle = 120f;
        public float visionRange = 12f;
        public bool requireLineOfSight = true;

        [Header("Steering Weights")]
        public float seekWeight = 1f;
        public float avoidWeight = 2f;
        public float separationWeight = 1.5f;
        public float orbitWeight = 0.8f;

        [Header("Steering Distances")]
        public float avoidDistance = 1.5f;
        public float sideAvoidDistance = 1.2f;
        public float separationRadius = 2f;

        [Header("Movement Feel")]
        public float orbitStrength = 0.6f;

        [Header("Behavior")]
        public bool enableSeparation = true;
        public bool enableAvoidance = true;
        public bool enableOrbit = true;
    }
}
