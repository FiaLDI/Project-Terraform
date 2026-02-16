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
            IBuffTarget[] raw = def.targetMode switch
            {
                TargetMode.Self => ResolveSelf(ctx),
                TargetMode.Area => ResolveArea(def, ctx),
                TargetMode.Directional => ResolveDirectional(def, ctx),
                _ => System.Array.Empty<IBuffTarget>()
            };

            return ApplyOwnershipFilter(raw, def, ctx);
        }

        // =====================================================
        // BASE RESOLUTION
        // =====================================================

        private static IBuffTarget[] ResolveSelf(EffectContext ctx)
        {
            return ctx.Source is IBuffTarget self
                ? new[] { self }
                : System.Array.Empty<IBuffTarget>();
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

        // =====================================================
        // OWNERSHIP FILTER
        // =====================================================

        private static IBuffTarget[] ApplyOwnershipFilter(
            IBuffTarget[] targets,
            EffectDefinition def,
            EffectContext ctx)
        {
            if (def.ownership == OwnershipFilter.Any ||
                targets.Length == 0)
                return targets;

            var filtered = new List<IBuffTarget>();

            foreach (var t in targets)
            {
                if (t == null)
                    continue;

                var owner = t.OwnerSource;
                var source = ctx.Source;

                if (owner == null || source == null)
                    continue;

                if (def.ownership == OwnershipFilter.SameOwner &&
                    owner == source)
                {
                    filtered.Add(t);
                }
                else if (def.ownership == OwnershipFilter.DifferentOwner &&
                         owner != source)
                {
                    filtered.Add(t);
                }
            }

            return filtered.ToArray();
        }
    }
}
