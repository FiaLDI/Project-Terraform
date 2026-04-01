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

            raw = ApplyOwnershipFilter(raw, def, ctx);

            if (def.coneAngle > 0f)
            {
                raw = ApplyConeFilter(raw, def, ctx);
            }

            if (def.selectClosest)
            {
                raw = SelectClosest(raw, ctx);
            }

            return raw;
        }

        // =====================================================
        // BASE
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
                AddFromCollider(h, results);

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
                AddFromCollider(hit.collider, results);
            }

            return results.ToArray();
        }

        // =====================================================
        // 🎯 CONE FILTER
        // =====================================================

       private static IBuffTarget[] ApplyConeFilter(
            IBuffTarget[] targets,
            EffectDefinition def,
            EffectContext ctx)
        {
            var results = new List<IBuffTarget>();

            Vector3 origin = ctx.Origin;
            Vector3 forward = ctx.Direction.normalized;

            float halfAngle = def.coneAngle * 0.5f + 25f;

            foreach (var t in targets)
            {
                if (t == null)
                    continue;

                Vector3 toTarget = t.Transform.position - origin;
                float dist = toTarget.magnitude;

                if (dist < 2.0f)
                {
                    results.Add(t);
                    continue;
                }

                Vector3 dir = toTarget.normalized;

                float angle = Vector3.Angle(forward, dir);

                if (angle <= halfAngle)
                    results.Add(t);
            }

            return results.ToArray();
        }

        // =====================================================
        // 🧠 TARGET SELECTION
        // =====================================================

        private static IBuffTarget[] SelectClosest(
            IBuffTarget[] targets,
            EffectContext ctx)
        {
            IBuffTarget best = null;
            float bestDist = float.MaxValue;

            foreach (var t in targets)
            {
                if (t == null)
                    continue;

                var go = t.BuffSystem?.gameObject;
                if (go == null)
                    continue;

                float dist =
                    Vector3.Distance(ctx.Origin, go.transform.position);

                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = t;
                }
            }

            return best != null
                ? new[] { best }
                : System.Array.Empty<IBuffTarget>();
        }

        // =====================================================
        // CORE (FIX INTERFACES)
        // =====================================================

        private static void AddFromCollider(
            Collider col,
            List<IBuffTarget> results)
        {
            var target = col.GetComponentInParent<StatsBuffTarget>();

            if (target != null)
            {
                if (!results.Contains(target))
                    results.Add(target);

                return;
            }
        }

        // =====================================================
        // OWNERSHIP
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
