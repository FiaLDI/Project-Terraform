using UnityEngine;
using Features.Stats.Domain;
using Features.Stats.Adapter;

namespace Features.Stats.UnityIntegration
{
    /// <summary>
    /// Предоставляет UI-доступ к IEnergyStats через IEnergyView.
    /// Вешается на Player HUD или объект с StatsFacadeAdapter.
    /// </summary>
    public class EnergyViewProvider : MonoBehaviour, IEnergyView
    {
        [SerializeField] private StatsFacadeAdapter statsAdapter;

        private IEnergyStats _energy;

        public float MaxEnergy => _energy?.MaxEnergy ?? 0f;
        public float CurrentEnergy => _energy?.CurrentEnergy ?? 0f;
        public float Regen => _energy?.Regen ?? 0f;
        public float CostMultiplier => _energy?.CostMultiplier ?? 1f;

        public event System.Action<float, float> OnEnergyChanged;

        private void Awake()
        {
            if (!statsAdapter)
                statsAdapter = GetComponentInParent<StatsFacadeAdapter>();
        }

        private void Start()
        {
            if (statsAdapter == null || statsAdapter.Stats == null)
            {
                Debug.LogError("[EnergyViewProvider] No StatsFacadeAdapter found!");
                return;
            }

            _energy = statsAdapter.Stats.Energy;

            if (_energy != null)
            {
                _energy.OnEnergyChanged += Relay;
                Relay(_energy.CurrentEnergy, _energy.MaxEnergy);
            }
            else
            {
                Debug.LogError("[EnergyViewProvider] StatsFacade has no EnergyStats!");
            }
        }

        private void Relay(float cur, float max)
        {
            OnEnergyChanged?.Invoke(cur, max);
        }

        private void OnDestroy()
        {
            if (_energy != null)
                _energy.OnEnergyChanged -= Relay;
        }
    }
}
