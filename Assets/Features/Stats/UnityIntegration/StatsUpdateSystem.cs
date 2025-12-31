using UnityEngine;
using FishNet.Object;
using FishNet.Managing.Timing;
using Features.Stats.Domain;

namespace Features.Stats.UnityIntegration
{
    [DefaultExecutionOrder(-200)]
    public sealed class StatsUpdateSystem : NetworkBehaviour
    {
        [SerializeField] private bool useUnscaledTime = false;

        private IStatsCollection stats;
        private IEnergyStats energy;
        private IHealthStats health;

        private bool isReady;

        public override void OnStartServer()
        {
            base.OnStartServer();
            TryInitServer();

            TimeManager.OnTick += OnServerTick;
        }

        public override void OnStopServer()
        {
            base.OnStopServer();
            TimeManager.OnTick -= OnServerTick;
        }

        private void TryInitServer()
        {
            if (isReady)
                return;

            var ps = GetComponent<PlayerStats>();
            if (ps == null || !ps.IsReady || ps.Facade == null)
                return;

            stats = ps.Facade as IStatsCollection;
            if (stats == null)
                return;

            energy = stats.Energy;
            health = stats.Health;

            isReady = true;

            Debug.Log("[StatsUpdateSystem] SERVER initialized ✅", this);
        }

        private void OnServerTick()
        {
            if (!IsServerStarted)
                return;

            if (!isReady)
            {
                TryInitServer();
                return;
            }

            float dt = (float)TimeManager.TickDelta;

            if (energy != null && energy.Regen > 0f)
                energy.Recover(energy.Regen * dt);

            if (health != null && health.FinalRegen > 0f)
                health.Recover(health.FinalRegen * dt);

            stats.Tick(dt);
        }
    }
}
