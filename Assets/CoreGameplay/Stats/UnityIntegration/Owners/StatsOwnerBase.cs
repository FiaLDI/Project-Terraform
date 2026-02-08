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
            Facade = new StatsFacade(statsProfile);
            Facade.ResetAll();
            IsReady = true;
        }
    }
}
