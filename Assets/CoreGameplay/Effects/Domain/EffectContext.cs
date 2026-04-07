using UnityEngine;
using Features.Buffs.Domain;

namespace Features.Effects.Domain
{
    public class EffectContext
    {
        public IBuffSource Source;
        public IBuffTarget[] Targets;
        public Vector3 Origin;
        public Vector3 Direction;

        public EffectContext(
            IBuffSource source,
            IBuffTarget[] targets,
            Vector3 origin,
            Vector3 direction)
        {
            Source = source;
            Targets = targets;
            Origin = origin;
            Direction = direction;
        }

        public void Reset(
            IBuffSource source,
            IBuffTarget[] targets,
            Vector3 origin,
            Vector3 direction)
        {
            Source = source;
            Targets = targets;
            Origin = origin;
            Direction = direction;
        }

        public void Clear()
        {
            Source = null;
            Targets = null;
        }
    }
}
