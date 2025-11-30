using UnityEngine;

namespace Features.Abilities.Domain
{
    [CreateAssetMenu(menuName = "Game/Ability/Shield Grid")]
    public class ShieldGridAbilitySO : AbilitySO
    {
        public float radius = 8f;
        public float duration = 15f;
        public float damageReductionPercent = 50f;

        [Header("FX")]
        public GameObject shieldGridPrefab;
    }
}
