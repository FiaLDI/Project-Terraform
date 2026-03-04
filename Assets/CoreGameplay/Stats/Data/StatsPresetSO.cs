using UnityEngine;

namespace Features.Stats.Application
{
    [CreateAssetMenu(menuName = "Game/Stats/Preset")]
    public class StatsPresetSO : ScriptableObject
    {
        [System.Serializable]
        public class CombatBlock
        {
            public float baseDamageMultiplier = 1f;
            public float baseFireRate = 5f;
            public float baseSpread = 2f;
            public float baseAimSpread = 0.5f;
            public float baseRange = 100f;
            public float baseRecoil = 1f;
            public int baseMagazineSize = 30;
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
            public float baseRegen = 1f;   // <-- ДОБАВЛЕНО
        }

        [System.Serializable]
        public class MovementBlock
        {
            public float baseSpeed = 5f;
            public float walkSpeed = 4f;
            public float sprintSpeed = 8f;
            public float crouchSpeed = 2f;
            public float rotationSpeed = 0f;
            public float gravity = -40f;
            public float jumpHeight = 1.2f;
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
