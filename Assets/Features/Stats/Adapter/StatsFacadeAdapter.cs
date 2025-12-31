using UnityEngine;

namespace Features.Stats.Adapter
{
    /// <summary>
    /// Контейнер View-адаптеров.
    /// НЕ инициализирует статы.
    /// НЕ знает про домен.
    /// </summary>
    public sealed class StatsFacadeAdapter : MonoBehaviour
    {
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
            Debug.Assert(HealthStats != null, "HealthStatsAdapter MISSING");
        }
    }
}
