using System;
using Features.Buffs.Domain;

namespace Features.Buffs.Application
{
    public class BuffInstance
    {
        public BuffSO Config { get; }
        public IBuffTarget Target { get; }

        public float Remaining { get; private set; }
        public bool IsExpired => Remaining <= 0f;

        public int StackCount { get; set; } = 1;

        public float Duration => Config.duration;

        public float Progress01 =>
            Duration > 0 ? 1f - (Remaining / Duration) : 1f;

        public BuffInstance(BuffSO config, IBuffTarget target)
        {
            Config = config;
            Target = target;
            Remaining = config.duration;
        }

        public void Tick(float dt)
        {
            if (Config.duration > 0)
                Remaining -= dt;
        }

        public void Refresh()
        {
            Remaining = Config.duration;
        }

        public void SetDuration(float newDuration)
        {
            Remaining = newDuration;
        }
    }
}
