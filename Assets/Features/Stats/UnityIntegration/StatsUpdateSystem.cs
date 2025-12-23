using UnityEngine;
using Features.Stats.Domain;
using Features.Stats.UnityIntegration;

namespace Features.Stats.UnityIntegration
{
    /// <summary>
    /// Единая система обновления статов сущности.
    /// Обновляет энергию, хп, регенерацию и тики бафов.
    /// Работает ТОЛЬКО после PlayerStats.Init().
    /// </summary>
    public class StatsUpdateSystem : MonoBehaviour
    {
        [SerializeField] private bool useUnscaledTime = false;

        private IStatsCollection stats;
        private IEnergyStats energy;
        private IHealthStats health;

        private bool isReady;

        // =====================================================
        // LIFECYCLE
        // =====================================================

        private void Awake()
        {
            // Awake намеренно пуст
        }

        private void OnEnable()
        {
            PlayerStats.OnStatsReady += HandleStatsReady;
        }

        private void OnDisable()
        {
            PlayerStats.OnStatsReady -= HandleStatsReady;
        }

        // =====================================================
        // INIT
        // =====================================================

        private void HandleStatsReady(PlayerStats ps)
        {
            if (isReady)
                return;

            // Этот StatsUpdateSystem должен работать
            // ТОЛЬКО со статами этого же объекта
            if (ps.gameObject != gameObject)
                return;

            var facade = ps.Facade;
            if (facade == null)
            {
                Debug.LogError("[StatsUpdateSystem] StatsFacade is null", this);
                return;
            }

            stats = facade as IStatsCollection;
            if (stats == null)
            {
                Debug.LogError(
                    "[StatsUpdateSystem] StatsFacade does not implement IStatsCollection",
                    this
                );
                return;
            }

            energy = stats.Energy;
            health = stats.Health;

            isReady = true;
        }

        // =====================================================
        // UPDATE
        // =====================================================

        private void Update()
        {
            if (!isReady || stats == null)
                return;

            float dt = useUnscaledTime
                ? Time.unscaledDeltaTime
                : Time.deltaTime;

            // ================================
            // ENERGY REGEN
            // ================================
            if (energy != null && energy.Regen > 0f)
                energy.Recover(energy.Regen * dt);

            // ================================
            // HEALTH REGEN
            // ================================
            if (health != null && health.FinalRegen > 0f)
                health.Recover(health.FinalRegen * dt);

            // ================================
            // BUFF / PASSIVE TICK UPDATE
            // ================================
            stats.Tick(dt);
        }
    }
}
