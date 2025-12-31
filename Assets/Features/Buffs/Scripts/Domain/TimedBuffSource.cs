using UnityEngine;

namespace Features.Buffs.Domain
{
    /// <summary>
    /// Временный источник баффа.
    /// Используется для duration-based эффектов,
    /// где нужен уникальный source.
    /// </summary>
    public sealed class TimedBuffSource : IBuffSource
    {
        public object Owner { get; }
        public float Duration { get; }

        public TimedBuffSource(object owner, float duration)
        {
            Owner = owner;
            Duration = duration;
        }
    }
}
