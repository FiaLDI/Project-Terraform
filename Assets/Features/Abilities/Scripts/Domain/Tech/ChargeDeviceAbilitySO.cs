using UnityEngine;
using Features.Buffs.Domain;

namespace Features.Abilities.Domain
{
    [CreateAssetMenu(menuName = "Game/Ability/Charge Device")]
    public class ChargeDeviceAbilitySO : AbilitySO
    {
        [Header("Aura Buff")]
        public AreaBuffSO areaBuff;

        [Header("FX")]
        public GameObject chargeFxPrefab;
    }
}
