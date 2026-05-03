using Features.Effects.Domain;
using UnityEngine;

namespace Features.Enemy.Data
{
    [CreateAssetMenu(menuName = "Enemies/Combat Config")]
    public class EnemyCombatConfigSO : ScriptableObject
    {
        [Header("Attack")]
        public EnemyAttackType attackType = EnemyAttackType.Melee;
        public float attackRange = 2f;
        public float attackDamage = 10f;
        public float attackCooldown = 1.2f;
        public float attackDelay = 0.3f;
        public EffectDefinition attackEffect;

        [Header("Behavior")]
        public float attackEnterOffset = 0.5f;
        public float attackExitOffset = 1.0f;
        public float stopDistanceMultiplier = 0.7f;
    }
}
