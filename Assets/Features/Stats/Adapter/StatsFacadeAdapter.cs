using UnityEngine;
using Features.Stats.Domain;
using System;

namespace Features.Stats.Adapter
{
    public class StatsFacadeAdapter : MonoBehaviour
    {
        private IStatsFacade _stats;
        public IStatsFacade Stats => _stats;

        public CombatStatsAdapter CombatStats  { get; private set; }
        public EnergyStatsAdapter EnergyStats  { get; private set; }
        public HealthStatsAdapter HealthStats  { get; private set; }
        public MovementStatsAdapter MovementStats { get; private set; }
        public MiningStatsAdapter MiningStats { get; private set; }

        public EnergyViewAdapter EnergyView { get; private set; }
        public HealthViewAdapter HealthView { get; private set; }

        public void Init(IStatsFacade stats)
        {
            _stats = stats;

            CombatStats = gameObject.AddComponent<CombatStatsAdapter>();
            CombatStats.Init(stats.Combat);

            EnergyStats = gameObject.AddComponent<EnergyStatsAdapter>();
            EnergyStats.Init(stats.Energy);

            HealthStats = gameObject.AddComponent<HealthStatsAdapter>();
            HealthStats.Init(stats.Health);

            MovementStats = gameObject.AddComponent<MovementStatsAdapter>();
            MovementStats.Init(stats.Movement);

            MiningStats = gameObject.AddComponent<MiningStatsAdapter>();
            MiningStats.Init(stats.Mining);

            EnergyView = gameObject.AddComponent<EnergyViewAdapter>();
            EnergyView.Init(stats.Energy);

            HealthView = gameObject.AddComponent<HealthViewAdapter>();
            HealthView.Init(stats.Health);
        }
    }
}
