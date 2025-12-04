using System.Collections.Generic;
using UnityEngine;
using Features.Abilities.Domain;
using Features.Passives.Domain;

namespace Features.Classes.Data
{
    [CreateAssetMenu(menuName = "Orbis/Classes/Class Config")]
    public class PlayerClassConfigSO : ScriptableObject
    {
        [Header("Identity")]
        public string id;
        public string displayName;
        [TextArea] public string description;

        [Header("Health")]
        public float baseHp = 120f;
        public float baseShield = 0f;

        [Header("Energy")]
        public float baseMaxEnergy = 100f;
        public float baseEnergyRegen = 8f;

        [Header("Combat")]
        public float baseDamageMultiplier = 1f;

        [Header("Movement")]
        public float baseMoveSpeed = 5f;
        public float walkSpeed = 4f;        // ← добавить
        public float sprintSpeed = 8f;
        public float crouchSpeed = 1.5f;

        [Header("Mining")]
        public float miningMultiplier = 1f;

        [Header("Content")]
        public List<PassiveSO> passives;
        public List<AbilitySO> abilities;
    }
}
