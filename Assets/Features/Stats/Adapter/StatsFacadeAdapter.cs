using UnityEngine;
using Features.Stats.Domain;

namespace Features.Stats.Adapter
{
    public class StatsFacadeAdapter : MonoBehaviour
    {
        public bool IsReady { get; private set; }

        public CombatStatsAdapter CombatStats { get; private set; }
        public EnergyStatsAdapter EnergyStats { get; private set; }
        public HealthStatsAdapter HealthStats { get; private set; }
        public MovementStatsAdapter MovementStats { get; private set; }
        public MiningStatsAdapter MiningStats { get; private set; }

        private void Awake()
        {
            CombatStats   = GetComponent<CombatStatsAdapter>();
            EnergyStats   = GetComponent<EnergyStatsAdapter>();
            HealthStats   = GetComponent<HealthStatsAdapter>();
            MovementStats = GetComponent<MovementStatsAdapter>();
            MiningStats   = GetComponent<MiningStatsAdapter>();

            Debug.Assert(EnergyStats != null, "EnergyStatsAdapter MISSING");
        }

        public void Init(IStatsFacade stats)
        {
            Debug.Log("[StatsFacadeAdapter] Init START");

            if (stats == null)
            {
                Debug.LogError("[StatsFacadeAdapter] stats == NULL");
                return;
            }

            CombatStats?.Init(stats.Combat);
            EnergyStats?.Init(stats.Energy);
            HealthStats?.Init(stats.Health);
            MovementStats?.Init(stats.Movement);
            MiningStats?.Init(stats.Mining);

            IsReady = true;

            Debug.Log("[StatsFacadeAdapter] Init COMPLETE → IsReady = true");
        }


    }
}
