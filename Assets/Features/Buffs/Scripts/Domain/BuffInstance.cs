using Features.Buffs.Domain;
using UnityEngine;

namespace Features.Buffs.Application
{
public sealed class BuffInstance
{
    public BuffSO Config { get; }
    public IBuffTarget Target { get; }
    public IBuffSource Source { get; }
    public BuffLifetimeMode LifetimeMode { get; }

    public float Remaining { get; private set; }

    public bool IsExpired => LifetimeMode == BuffLifetimeMode.Duration && Remaining <= 0f;

    public float Duration => Config.duration;

    public float Progress01 =>
        Duration > 0f ? Mathf.Clamp01(1f - Remaining / Duration) : 1f;

    public BuffInstance(
        BuffSO config,
        IBuffTarget target,
        IBuffSource source,
        BuffLifetimeMode lifetimeMode)
    {
        Config = config;
        Target = target;
        Source = source;
        LifetimeMode = lifetimeMode;

        Remaining = lifetimeMode == BuffLifetimeMode.Duration
            ? config.duration
            : float.PositiveInfinity;
    }

    public void Tick(float dt)
    {
        if (LifetimeMode == BuffLifetimeMode.Duration)
            Remaining -= dt;
    }
}
}