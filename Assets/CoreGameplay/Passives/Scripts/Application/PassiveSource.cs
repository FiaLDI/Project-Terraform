using Features.Buffs.Domain;
using Features.Passives.Domain;

namespace Features.Passives.Application
{
    /// <summary>
    /// Runtime-источник бафов одной пассивки.
    /// Один PassiveSO = один IBuffSource.
    /// </summary>
    public sealed class PassiveSource : IBuffSource
    {
        public PassiveSO Passive { get; }

        public PassiveSource(PassiveSO passive)
        {
            Passive = passive;
        }
    }
}
