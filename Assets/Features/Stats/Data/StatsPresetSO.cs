using UnityEngine;

namespace Features.Stats.Application
{
    [CreateAssetMenu(menuName = "Orbis/Stats/Preset")]
    public class StatsPresetSO : ScriptableObject
    {
        [System.Serializable]
        public class CombatBlock
        {
            public float baseDamageMultiplier = 1f;
        }

        [System.Serializable]
        public class EnergyBlock
        {
            public float baseMaxEnergy = 100f;
            public float baseRegen = 8f;
        }

        [System.Serializable]
        public class HealthBlock
        {
            public float baseHp = 100f;
        }

        [System.Serializable]
        public class MovementBlock
        {
            public float baseSpeed = 5f;
            public float walkSpeed = 4f;
            public float sprintSpeed = 8f;
            public float crouchSpeed = 2f;
        }

        [System.Serializable]
        public class MiningBlock
        {
            public float baseMining = 1f;
        }

        [Header("Combat")]
        public CombatBlock combat = new CombatBlock();

        [Header("Energy")]
        public EnergyBlock energy = new EnergyBlock();

        [Header("Health")]
        public HealthBlock health = new HealthBlock();

        [Header("Movement")]
        public MovementBlock movement = new MovementBlock();

        [Header("Mining")]
        public MiningBlock mining = new MiningBlock();
    }
}
