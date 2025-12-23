using UnityEngine;
using Features.Stats.Domain;
using Features.Stats.Adapter;

namespace Features.Stats.UnityIntegration
{
    [DefaultExecutionOrder(-400)]
    public class PlayerStats : MonoBehaviour
    {
        public IStatsFacade Facade { get; private set; }
        public StatsFacadeAdapter Adapter { get; private set; }

        public bool IsReady { get; private set; }

        public static event System.Action<PlayerStats> OnStatsReady;

        private void Awake()
        {
            // Awake пустой — принципиально
        }

        public void Init()
        {
            if (IsReady)
                return;

            Facade = new StatsFacade(isTurret: false);

            Adapter = gameObject.AddComponent<StatsFacadeAdapter>();
            Adapter.Init(Facade);

            IsReady = true;

            OnStatsReady?.Invoke(this);
        }
    }
}
