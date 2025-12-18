using UnityEngine;
using Features.Buffs.Domain;

namespace Features.Abilities.Domain
{
    [CreateAssetMenu(menuName = "Game/Ability/Overload Pulse")]
    public class OverloadPulseAbilitySO : AbilitySO
    {
        [Header("Buffs applied to turrets")]
        public BuffSO damageBuff;
        public BuffSO fireRateBuff;
        public BuffSO turretMoveBuff;

        [Header("Pulse Params")]
        public float radius = 12f;
        public float pulseDamage = 35f;
        public float knockbackForce = 10f;

        [Header("FX")]
        public GameObject pulseFxPrefab;

        [Header("FX Lifetime")]
        public float fxDuration = 0.5f;
    }
}
