using System;

namespace Features.Stats.Domain
{
    [Serializable]
    public struct StatsSnapshot
    {
        public float energy;
        public float maxEnergy;

        public float health;
        public float maxHealth;
    }
}
