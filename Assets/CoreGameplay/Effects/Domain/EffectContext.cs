using Features.Buffs.Domain;

namespace Features.Effects.Domain
{
    public readonly struct EffectContext
    {
        public readonly IBuffSource Source;
        public readonly IBuffTarget[] Targets;
        public readonly UnityEngine.Vector3 Origin;
        public readonly UnityEngine.Vector3 Direction;

        public EffectContext(
            IBuffSource source,
            IBuffTarget[] targets,
            UnityEngine.Vector3 origin,
            UnityEngine.Vector3 direction)
        {
            Source = source;
            Targets = targets;
            Origin = origin;
            Direction = direction;
        }
    }
}
