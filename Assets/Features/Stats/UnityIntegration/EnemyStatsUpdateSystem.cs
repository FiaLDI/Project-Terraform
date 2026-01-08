using FishNet.Object;
using FishNet.Managing.Timing;
using Features.Stats.Domain;
using UnityEngine;


namespace Features.Enemy.UnityIntegration
{
    [DefaultExecutionOrder(-200)]
    public sealed class EnemyStatsUpdateSystem : NetworkBehaviour
    {
        private IStatsCollection stats;
        private IHealthStats health;
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

            var es = GetComponent<EnemyStats>();
            if (es == null || !es.IsReady)
                return;

            stats = es.Facade as IStatsCollection;
            health = stats.Health;

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

            if (health.FinalRegen > 0f)
                health.Recover(health.FinalRegen * dt);

            stats.Tick(dt);
        }
    }
}
