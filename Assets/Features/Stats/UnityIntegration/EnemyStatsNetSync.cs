using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Managing.Timing;
using Features.Stats.Domain;

namespace Features.Enemy.UnityIntegration
{
    public sealed class EnemyStatsNetSync : NetworkBehaviour
    {
        private readonly SyncVar<float> syncedHp = new();
        private readonly SyncVar<float> syncedMaxHp = new();

        private IHealthStats health;

        public override void OnStartServer()
        {
            base.OnStartServer();

            var stats = GetComponent<EnemyStats>()?.Facade;
            health = stats?.Health;

            TimeManager.OnTick += OnTick;
        }

        public override void OnStopServer()
        {
            base.OnStopServer();
            TimeManager.OnTick -= OnTick;
        }

        private void OnTick()
        {
            if (!IsServerStarted || health == null)
                return;

            syncedHp.Value = health.CurrentHp;
            syncedMaxHp.Value = health.MaxHp;
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            syncedHp.OnChange += OnHpChanged;
        }

        public override void OnStopClient()
        {
            base.OnStopClient();
            syncedHp.OnChange -= OnHpChanged;
        }

        private void OnHpChanged(float oldValue, float newValue, bool asServer)
        {
            if (asServer) return;

            var view = GetComponent<EnemyHealth>();
            if (view != null)
                view.SetHealthFromNetwork(newValue, syncedMaxHp.Value);
        }
    }
}
