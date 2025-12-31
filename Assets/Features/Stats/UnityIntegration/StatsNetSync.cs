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

        // ================= REFERENCES =================
        private IStatsFacade stats;              // SERVER ONLY
        private StatsFacadeAdapter adapter;       // CLIENT ONLY

        // ================= SETTINGS =================
        [Header("Network Optimization")]
        [SerializeField] private float syncInterval = 0.1f;
        [SerializeField] private float threshold = 0.05f;

        [Header("Interpolation (Client)")]
        [SerializeField] private float lerpSpeed = 12f;

        // ================= SERVER =================
        private float serverTimer;
        private StatsSnapshot lastSent;

        // ================= CLIENT =================
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
        // SERVER LIFECYCLE
        // =====================================================

        public override void OnStartServer()
        {
            base.OnStartServer();

            stats = GetComponent<PlayerStats>()?.Facade;
            if (stats == null)
            {
                Debug.LogError("[StatsNetSync] StatsFacade not found on SERVER", this);
                return;
            }

            // 🔥 КАНОНИЧНО: серверный тик ТОЛЬКО через TimeManager
            TimeManager.OnTick += OnServerTick;
        }

        public override void OnStopServer()
        {
            base.OnStopServer();
            TimeManager.OnTick -= OnServerTick;
        }

        // =====================================================
        // CLIENT LIFECYCLE
        // =====================================================

        public override void OnStartClient()
        {
            base.OnStartClient();

            adapter = GetComponent<StatsFacadeAdapter>();
            if (adapter == null)
            {
                Debug.LogError("[StatsNetSync] StatsFacadeAdapter not found on CLIENT", this);
                return;
            }

            syncedStats.OnChange += OnStatsChanged;
        }

        public override void OnStopClient()
        {
            base.OnStopClient();
            syncedStats.OnChange -= OnStatsChanged;
        }

        // =====================================================
        // SERVER TICK (ЕДИНСТВЕННЫЙ ИСТОЧНИК СИНКА)
        // =====================================================

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
        // CLIENT UPDATE (ТОЛЬКО ВИЗУАЛ)
        // =====================================================

        private void Update()
        {
            if (!IsClientStarted || !hasInitial || adapter == null)
                return;

            currentEnergy = Mathf.Lerp(currentEnergy, targetEnergy, Time.deltaTime * lerpSpeed);
            currentHealth = Mathf.Lerp(currentHealth, targetHealth, Time.deltaTime * lerpSpeed);

            ApplyToAdapters();
        }

        // =====================================================
        // SERVER HELPERS
        // =====================================================

        private StatsSnapshot BuildSnapshot()
        {
            return new StatsSnapshot
            {
                energy    = stats.Energy.CurrentEnergy,
                maxEnergy = stats.Energy.MaxEnergy,
                health    = stats.Health.CurrentHp,
                maxHealth = stats.Health.MaxHp
            };
        }

        private bool HasMeaningfulChange(StatsSnapshot a, StatsSnapshot b)
        {
            return
                Mathf.Abs(a.energy - b.energy) > threshold ||
                Mathf.Abs(a.maxEnergy - b.maxEnergy) > threshold ||
                Mathf.Abs(a.health - b.health) > threshold ||
                Mathf.Abs(a.maxHealth - b.maxHealth) > threshold;
        }

        // =====================================================
        // CLIENT RECEIVE
        // =====================================================

        private void OnStatsChanged(StatsSnapshot oldValue, StatsSnapshot newValue, bool asServer)
        {
            if (asServer)
                return;

            targetEnergy = newValue.energy;
            targetHealth = newValue.health;

            maxEnergy = newValue.maxEnergy;
            maxHealth = newValue.maxHealth;

            if (!hasInitial)
            {
                currentEnergy = targetEnergy;
                currentHealth = targetHealth;
                hasInitial = true;
            }

            if (newValue.health <= 0f)
                currentHealth = 0f;

            ApplyToAdapters();
        }

        // =====================================================
        // APPLY TO UI / ADAPTERS
        // =====================================================

        private void ApplyToAdapters()
        {
            if (adapter == null)
                return;

            if (adapter.EnergyStats != null)
            {
                float energyMax = energyMaxGuard.ShouldApply(maxEnergy)
                    ? maxEnergy
                    : energyMaxGuard.Current;

                adapter.EnergyStats.Set(currentEnergy, energyMax);
            }

            if (adapter.HealthStats != null)
            {
                float healthMax = healthMaxGuard.ShouldApply(maxHealth)
                    ? maxHealth
                    : healthMaxGuard.Current;

                adapter.HealthStats.SetHp(currentHealth, healthMax);
            }
        }
    }
}
