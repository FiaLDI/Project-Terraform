using UnityEngine;
using System.Collections.Generic;
using Features.Effects.Domain;
using Features.Buffs.Domain;

namespace Features.Effects.Application
{
    public static class TargetResolver
    {
        public static IBuffTarget[] Resolve(
            EffectDefinition def,
            EffectContext ctx)
        {
            switch (def.targetMode)
            {
                case TargetMode.Self:
                    return ctx.Source is IBuffTarget self
                        ? new[] { self }
                        : System.Array.Empty<IBuffTarget>();

                case TargetMode.Area:
                    return ResolveArea(def, ctx);

                case TargetMode.Directional:
                    return ResolveDirectional(def, ctx);

                default:
                    return System.Array.Empty<IBuffTarget>();
            }
        }

        private static IBuffTarget[] ResolveArea(
            EffectDefinition def,
            EffectContext ctx)
        {
            var results = new List<IBuffTarget>();

            var hits = Physics.OverlapSphere(
                ctx.Origin,
                def.radius,
                def.layerMask);

            foreach (var h in hits)
            {
                if (h.TryGetComponent<IBuffTarget>(out var target))
                    results.Add(target);
            }

            return results.ToArray();
        }

        private static IBuffTarget[] ResolveDirectional(
            EffectDefinition def,
            EffectContext ctx)
        {
            var results = new List<IBuffTarget>();

            if (Physics.Raycast(
                ctx.Origin,
                ctx.Direction,
                out RaycastHit hit,
                def.radius,
                def.layerMask))
            {
                if (hit.collider.TryGetComponent<IBuffTarget>(out var target))
                    results.Add(target);
            }

            return results.ToArray();
        }
    }
}
