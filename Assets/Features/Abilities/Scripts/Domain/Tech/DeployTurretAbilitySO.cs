using UnityEngine;

namespace Features.Abilities.Domain
{
    [CreateAssetMenu(menuName = "Game/Ability/Deploy Turret")]
    public class DeployTurretAbilitySO : AbilitySO
    {
        [Header("Turret Stats")]
        public GameObject turretPrefab;
        public float duration = 25f;
        public float damagePerSecond = 4f;
        public float range = 15f;
        public int hp = 150;
    }
}
