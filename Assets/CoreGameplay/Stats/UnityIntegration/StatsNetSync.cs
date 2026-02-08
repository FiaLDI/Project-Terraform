using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Managing.Timing;
using Features.Stats.Domain;
using Features.Stats.Adapter;
using Features.Stats.Net;

namespace Features.Stats.UnityIntegration
{
    public sealed class StatsNetSync : NetworkBehaviour
    {
        // ================= NETWORK =================
        private readonly SyncVar<StatsSnapshot> syncedStats = new();

        // ================= CONFIG =================
        [SerializeField] private StatsNetProfileSO netProfile;

        [Header("Network Optimization")]
        [SerializeField] private float syncInterval = 0.1f;
        [SerializeField] private float threshold = 0.05f;

        [Header("Interpolation (Client)")]
        [SerializeField] private float lerpSpeed = 12f;

        // ================= SERVER =================
        private IStatsFacade stats;
        private float serverTimer;
        private StatsSnapshot lastSent;

        // ================= CLIENT =================
        private StatsFacadeAdapter adapter;

        private float currentEnergy;
        private float currentHealth;

        private float targetEnergy;
        private float targetHealth;

        private float maxEnergy;
        private float maxHealth;

        private bool hasInitial;

        private readonly StatApplyGuard energyMaxGuard = new();
        private readonly StatApplyGuard healthMaxGuard = new();

        // =====================================================
        // SERVER
        // =====================================================

        public override void OnStartServer()
        {
            base.OnStartServer();

            if (netProfile == null || netProfile.mode == StatsNetMode.None)
                return;

            var owner = GetComponent<IStatsOwner>();
            stats = owner?.Facade;

            if (stats == null)
            {
                Debug.LogError("[StatsNetSync] StatsOwner not found", this);
                return;
            }

            TimeManager.OnTick += OnServerTick;
        }

        public override void OnStopServer()
        {
            base.OnStopServer();
            TimeManager.OnTick -= OnServerTick;
        }

        private void OnServerTick()
        {
            if (!IsServerStarted || stats == null)
                return;

            serverTimer += (float)TimeManager.TickDelta;
            if (serverTimer < syncInterval)
                return;

            serverTimer = 0f;

            var snap = BuildSnapshot();
            if (!HasMeaningfulChange(lastSent, snap))
                return;

            lastSent = snap;
            syncedStats.Value = snap;
        }

        // =====================================================
        // CLIENT
        // =====================================================

        public override void OnStartClient()
        {
            base.OnStartClient();

            if (netProfile == null || netProfile.mode == StatsNetMode.None)
                return;

            adapter = GetComponent<StatsFacadeAdapter>();
            if (adapter == null)
            {
                Debug.LogError("[StatsNetSync] StatsFacadeAdapter missing", this);
                return;
            }

            syncedStats.OnChange += OnStatsChanged;
        }

        public override void OnStopClient()
        {
            base.OnStopClient();
            syncedStats.OnChange -= OnStatsChanged;
        }

        private void Update()
        {
            if (!IsClientStarted || !hasInitial || adapter == null)
                return;

            currentHealth = Mathf.Lerp(currentHealth, targetHealth, Time.deltaTime * lerpSpeed);
            currentEnergy = Mathf.Lerp(currentEnergy, targetEnergy, Time.deltaTime * lerpSpeed);

            ApplyToAdapters();
        }

        // =====================================================
        // SNAPSHOT
        // =====================================================

        private StatsSnapshot BuildSnapshot()
        {
            var snap = new StatsSnapshot();

            if (netProfile.mode == StatsNetMode.Full)
            {
                snap.energy    = stats.Energy?.CurrentEnergy ?? 0f;
                snap.maxEnergy = stats.Energy?.MaxEnergy ?? 0f;
            }

            if (netProfile.mode >= StatsNetMode.HealthOnly)
            {
                snap.health    = stats.Health?.CurrentHp ?? 0f;
                snap.maxHealth = stats.Health?.MaxHp ?? 0f;
            }

            return snap;
        }

        private bool HasMeaningfulChange(StatsSnapshot a, StatsSnapshot b)
        {
            return
                Mathf.Abs(a.health - b.health) > threshold ||
                Mathf.Abs(a.maxHealth - b.maxHealth) > threshold ||
                Mathf.Abs(a.energy - b.energy) > threshold ||
                Mathf.Abs(a.maxEnergy - b.maxEnergy) > threshold;
        }

        // =====================================================
        // CLIENT RECEIVE
        // =====================================================

        private void OnStatsChanged(StatsSnapshot oldValue, StatsSnapshot newValue, bool asServer)
        {
            if (asServer)
                return;

            targetHealth = newValue.health;
            maxHealth    = newValue.maxHealth;

            targetEnergy = newValue.energy;
            maxEnergy    = newValue.maxEnergy;

            if (!hasInitial)
            {
                currentHealth = targetHealth;
                currentEnergy = targetEnergy;
                hasInitial = true;
            }

            ApplyToAdapters();
        }

        private void ApplyToAdapters()
        {
            if (adapter.HealthStats != null)
            {
                float maxHp = healthMaxGuard.ShouldApply(maxHealth)
                    ? maxHealth
                    : healthMaxGuard.Current;

                adapter.HealthStats.SetHp(currentHealth, maxHp);
            }

            if (adapter.EnergyStats != null && netProfile.mode == StatsNetMode.Full)
            {
                float maxEn = energyMaxGuard.ShouldApply(maxEnergy)
                    ? maxEnergy
                    : energyMaxGuard.Current;

                adapter.EnergyStats.Set(currentEnergy, maxEn);
            }
        }
    }
}
