using UnityEngine;
using Features.Stats.Domain;

public class EnergyViewAdapter : MonoBehaviour, IEnergyView
{
    private IEnergyStats stats;

    public float MaxEnergy => stats?.MaxEnergy ?? 0;
    public float CurrentEnergy => stats?.CurrentEnergy ?? 0;
    public float Regen => stats?.Regen ?? 0;
    public float CostMultiplier => stats?.CostMultiplier ?? 1;

    public event System.Action<float, float> OnEnergyChanged;

    public void Init(IEnergyStats s)
    {
        stats = s;

        stats.OnEnergyChanged += Relay;
        Relay(stats.CurrentEnergy, stats.MaxEnergy);
    }

    private void Relay(float cur, float max)
    {
        OnEnergyChanged?.Invoke(cur, max);
    }

    private void OnDestroy()
    {
        if (stats != null)
            stats.OnEnergyChanged -= Relay;
    }
}
