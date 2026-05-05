using UnityEngine;

namespace Features.Enemy.Data
{
    [CreateAssetMenu(menuName = "Enemies/AI Config")]
    public class EnemyAIConfigSO : ScriptableObject
    {
        [Header("Aggro")]
        public float aggroRadius = 12f;
        public float loseAggroRadius = 18f;
        public float threatDecayPerSecond = 4f;
        public float targetSwitchThreshold = 1.2f;
        public float currentTargetBias = 2f;
        public float aggroConfirmTime = 0.3f;

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
        public float directionSmoothing = 8f;

        [Header("Brain")]
        public float lostSightGraceTime = 0.25f;
        public float attackMoveGoalTolerance = 0.15f;
        public float returnReachDistance = 1f;
        public float preferredCombatDistance = 0f;
        public float retreatDistance = 0f;
        public float reengageDistance = 0f;

        [Header("Behavior Toggles")]
        public bool enableSeparation = true;
        public bool enableAvoidance = true;
        public bool enableOrbit = true;
    }
}
