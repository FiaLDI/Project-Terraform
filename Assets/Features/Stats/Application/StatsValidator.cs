using UnityEngine;
using Features.Stats.Domain;

namespace Features.Stats.Application
{
    /// <summary>
    /// Помощник для проверки расхождений между сервером и клиентом
    /// </summary>
    public static class StatsValidator
    {
        private const float TOLERANCE = 0.01f;

        public static bool CompareHealth(IHealthStats health, float expectedMaxHp, float expectedCurrentHp)
        {
            bool maxMatch = Mathf.Abs(health.MaxHp - expectedMaxHp) <= TOLERANCE;
            bool currentMatch = Mathf.Abs(health.CurrentHp - expectedCurrentHp) <= TOLERANCE;

            if (!maxMatch)
                Debug.LogWarning($"[StatsValidator] MaxHp mismatch: {health.MaxHp} != {expectedMaxHp}");
            
            if (!currentMatch)
                Debug.LogWarning($"[StatsValidator] CurrentHp mismatch: {health.CurrentHp} != {expectedCurrentHp}");

            return maxMatch && currentMatch;
        }

        public static bool CompareEnergy(IEnergyStats energy, float expectedMaxEnergy, float expectedCurrentEnergy)
        {
            bool maxMatch = Mathf.Abs(energy.MaxEnergy - expectedMaxEnergy) <= TOLERANCE;
            bool currentMatch = Mathf.Abs(energy.CurrentEnergy - expectedCurrentEnergy) <= TOLERANCE;

            if (!maxMatch)
                Debug.LogWarning($"[StatsValidator] MaxEnergy mismatch: {energy.MaxEnergy} != {expectedMaxEnergy}");
            
            if (!currentMatch)
                Debug.LogWarning($"[StatsValidator] CurrentEnergy mismatch: {energy.CurrentEnergy} != {expectedCurrentEnergy}");

            return maxMatch && currentMatch;
        }

        public static string GetStatsDebugInfo(IStatsFacade stats)
        {
            if (stats == null) return "Stats is NULL";

            var h = stats.Health;
            var e = stats.Energy;
            var m = stats.Movement;
            var c = stats.Combat;

            return $"HP={h.CurrentHp:0.0}/{h.MaxHp:0.0} " +
                   $"EN={e.CurrentEnergy:0.0}/{e.MaxEnergy:0.0} " +
                   $"Speed={m.BaseSpeed:0.0} " +
                   $"Dmg={c.DamageMultiplier:0.0}x";
        }
    }
}
