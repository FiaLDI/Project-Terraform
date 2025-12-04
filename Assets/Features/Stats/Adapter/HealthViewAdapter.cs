using UnityEngine;
using Features.Stats.Domain;

public class HealthViewAdapter : MonoBehaviour
{
    private IHealthStats stats;

    public float MaxHp => stats?.MaxHp ?? 0;
    public float CurrentHp => stats?.CurrentHp ?? 0;

    public event System.Action<float, float> OnHealthChanged;

    public void Init(IHealthStats s)
    {
        stats = s;

        stats.OnHealthChanged += Relay;
        Relay(stats.CurrentHp, stats.MaxHp);
    }

    private void Relay(float cur, float max)
    {
        OnHealthChanged?.Invoke(cur, max);
    }

    private void OnDestroy()
    {
        if (stats != null)
            stats.OnHealthChanged -= Relay;
    }
}
