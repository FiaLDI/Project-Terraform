
using Features.Stats.Domain;
using FishNet.Object;
using UnityEngine;

namespace Features.Stats.UnityIntegration
{
    [DefaultExecutionOrder(-200)]
    public sealed class UnifiedStatsUpdateSystem : NetworkBehaviour
    {
        private IStatsCollection stats;
        private IHealthStats health;
        private IEnergyStats energy;

        private bool ready;

        public override void OnStartServer()
        {
            base.OnStartServer();
            TimeManager.OnTick += OnTick;
        }

        public override void OnStopServer()
        {
            base.OnStopServer();
            TimeManager.OnTick -= OnTick;
        }

        private void TryInit()
        {
            if (ready) return;

            var owner = GetComponent<IStatsOwner>();
            if (owner == null || !owner.IsReady)
                return;

            stats = owner.Facade as IStatsCollection;
            health = stats?.Health;
            energy = stats?.Energy;

            ready = true;
        }

        private void OnTick()
        {
            if (!IsServerStarted)
                return;

            if (!ready)
            {
                TryInit();
                return;
            }

            float dt = (float)TimeManager.TickDelta;

            if (energy != null && energy.Regen > 0)
                energy.Recover(energy.Regen * dt);

            if (health != null && health.FinalRegen > 0)
                health.Recover(health.FinalRegen * dt);

            stats.Tick(dt);
        }
    }
}