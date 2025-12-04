using UnityEngine;
using Features.Stats.Domain;

public class HealthViewAdapter : MonoBehaviour
{
    private IHealthStats _stats;

    public float CurrentHp => _stats?.CurrentHp ?? 0f;
    public float MaxHp => _stats?.MaxHp ?? 0f;

    public event System.Action<float, float> OnHpChanged;

    public void Init(IHealthStats stats)
    {
        _stats = stats;

        stats.OnHealthChanged += (cur, max) =>
        {
            OnHpChanged?.Invoke(cur, max);
        };
    }
}
