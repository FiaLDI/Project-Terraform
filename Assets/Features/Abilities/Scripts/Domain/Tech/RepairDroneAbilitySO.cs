using UnityEngine;
using Features.Buffs.Domain;

namespace Features.Abilities.Domain
{
    [CreateAssetMenu(menuName = "Game/Ability/Repair Drone")]
    public class RepairDroneAbilitySO : AbilitySO
    {
        [Header("Aura Buff")]
        public AreaBuffSO healAura;

        [Header("Drone Settings")]
        public float lifetime = 10f;
        public float followSpeed = 6f;

        [Header("FX / Drone Prefab")]
        public GameObject dronePrefab;
    }
}
