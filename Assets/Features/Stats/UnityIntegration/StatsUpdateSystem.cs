using UnityEngine;
using Features.Stats.Adapter;
using Features.Stats.Domain;

namespace Features.Stats.UnityIntegration
{
    /// <summary>
    /// Единая система обновления статов сущности.
    /// Обновляет энергию, хп, щиты, регенерацию и тики бафов.
    /// Висит на Player / NPC / любом объекте со статами.
    /// </summary>
    public class StatsUpdateSystem : MonoBehaviour
    {
        [SerializeField] private StatsFacadeAdapter statsAdapter;
        [SerializeField] private bool useUnscaledTime = false;

        private IStatsCollection stats;

        private IEnergyStats energy;
        private IHealthStats health;

        private void Awake()
        {
            if (statsAdapter == null)
                statsAdapter = GetComponent<StatsFacadeAdapter>();
        }

        private void Start()
        {
            if (statsAdapter == null || statsAdapter.Stats == null)
            {
                Debug.LogError("[StatsUpdateSystem] No StatsFacadeAdapter or Stats found!");
                enabled = false;
                return;
            }

            stats = statsAdapter.Stats as IStatsCollection;

            if (stats == null)
            {
                Debug.LogError("StatsFacade does not implement IStatsCollection!");
                enabled = false;
                return;
            }

            energy = stats.Energy;
            health = stats.Health;
        }

        private void Update()
        {
            if (stats == null) return;

            float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

            // ================================
            // ENERGY REGEN
            // ================================
            if (energy != null && energy.Regen > 0f)
                energy.Recover(energy.Regen * dt);

            // ================================
            // HEALTH REGEN (если есть)
            // ================================
            if (health != null && health.Regen > 0f)
                health.Recover(health.Regen * dt);

            // ================================
            // SHIELD REGEN (если есть)
            // ================================

            // ================================
            // BUFF / PASSIVE TICK UPDATE
            // ================================
            stats.Tick(dt);
        }
    }
}
