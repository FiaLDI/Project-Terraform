using UnityEngine;

namespace Features.Stats.Data
{
    [CreateAssetMenu(menuName = "Game/Stats/Profile")]
    public class StatsProfileSO : ScriptableObject
    {
        public bool hasHealth = true;
        public bool hasEnergy = true;
        public bool hasCombat = true;
        public bool hasMovement = true;
        public bool hasMining = false;

        [Header("Combat")]
        public bool useTurretCombat;
    }
}