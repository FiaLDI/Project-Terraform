using UnityEngine;

namespace Features.Abilities.Domain
{
    [CreateAssetMenu(menuName = "Game/Ability/Deploy Turret v2")]
    public class DeployTurretAbilitySO : AbilitySO
    {
        [Header("Turret Base")]
        public GameObject turretPrefab;
        public float duration = 25f;
        public float range = 15f;

        [Header("Stats")]
        [Tooltip("Максимальное здоровье турели")]
        public float hp = 150f;

        [Tooltip("Множитель урона (1 = базовый урон из TurretPreset, 2 = x2 урон)")]
        public float damageMultiplier = 1f;

        [Tooltip("Множитель скорострельности (1 = базовая, 2 = в 2 раза быстрее)")]
        public float fireRate = 1f;
    }
}
