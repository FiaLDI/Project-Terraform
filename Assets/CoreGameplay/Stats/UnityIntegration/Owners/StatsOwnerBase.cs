using FishNet.Object;
using Features.Stats.Domain;
using UnityEngine;
using Features.Stats.Data;

namespace Features.Stats.UnityIntegration
{
    [DefaultExecutionOrder(-400)]
    public abstract class StatsOwnerBase : NetworkBehaviour, IStatsOwner
    {
        [Header("Stats")]
        [SerializeField] protected StatsProfileSO statsProfile;

        public IStatsFacade Facade { get; protected set; }
        public bool IsReady { get; protected set; }

        public override void OnStartServer()
        {
            base.OnStartServer();
            InitStats();
        }

        protected virtual void InitStats()
        {
            if (statsProfile == null)
            {
                Debug.LogError("[StatsOwnerBase] StatsProfileSO missing", this);
                return;
            }

            // =========================
            // CREATE SUB-STATS
            // =========================

            IHealthStats health = statsProfile.hasHealth
                ? new HealthStats()
                : null;

            IEnergyStats energy = statsProfile.hasEnergy
                ? new EnergyStats()
                : null;

            ICombatStats combat = statsProfile.hasCombat
                ? (statsProfile.useTurretCombat
                    ? new TurretCombatStats()
                    : new CombatStats())
                : null;

            IMovementStats movement = statsProfile.hasMovement
                ? new MovementStats()
                : null;

            IMiningStats mining = statsProfile.hasMining
                ? new MiningStats()
                : null;

            // =========================
            // CREATE FACADE
            // =========================

            Facade = new StatsFacade(
                health,
                energy,
                combat,
                movement,
                mining
            );

            Facade.ResetAll();
            IsReady = true;
        }
    }
}
