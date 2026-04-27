using UnityEngine;
using System;
using Features.Effects.Domain;
using Features.Buffs.Domain;

namespace Features.Effects.Application
{
    public static class TargetResolver
    {
        private static readonly Collider[] colliderBuffer = new Collider[64];
        private static readonly IBuffTarget[] targetBuffer = new IBuffTarget[64];
        private static readonly IBuffTarget[] filterBuffer = new IBuffTarget[64];

        public static IBuffTarget[] Resolve(
            EffectDefinition def,
            EffectContext ctx)
        {
            int count = 0;

            // ================= BASE =================

            switch (def.targetMode)
            {
                case TargetMode.Self:
                    if (ctx.Source is IBuffTarget self)
                    {
                        targetBuffer[0] = self;
                        count = 1;
                    }
                    break;

                case TargetMode.Area:
                    count = ResolveArea(def, ctx);
                    break;

                case TargetMode.Directional:
                    count = ResolveDirectional(def, ctx);
                    break;

                case TargetMode.Explicit:
                    return ctx.Targets ?? Array.Empty<IBuffTarget>();
            }

            if (count == 0)
                return Array.Empty<IBuffTarget>();

            // ================= OWNERSHIP =================

            count = ApplyOwnershipFilter(targetBuffer, filterBuffer, count, def, ctx);

            if (count == 0)
                return Array.Empty<IBuffTarget>();

            // ================= CONE =================

            if (def.coneAngle > 0f)
            {
                count = ApplyConeFilter(filterBuffer, targetBuffer, count, def, ctx);
                if (count == 0)
                    return Array.Empty<IBuffTarget>();
            }

            // ================= CLOSEST =================

            if (def.selectClosest)
            {
                var best = SelectClosest(targetBuffer, count, ctx);
                if (best == null)
                    return Array.Empty<IBuffTarget>();

                return new[] { best }; // допустимая 1 аллокация
            }

            // ================= FINAL COPY =================

            var result = new IBuffTarget[count];
            Array.Copy(targetBuffer, result, count);
            return result;
        }

        // =====================================================
        // AREA (NonAlloc)
        // =====================================================

        private static int ResolveArea(EffectDefinition def, EffectContext ctx)
        {
            int hits = Physics.OverlapSphereNonAlloc(
                ctx.Origin,
                def.radius,
                colliderBuffer,
                def.layerMask);

            int count = 0;

            for (int i = 0; i < hits; i++)
            {
                var col = colliderBuffer[i];
                if (col == null)
                    continue;

                IBuffTarget target = ExtractTarget(col);

                if (target == null)
                    continue;

                // дедуп без Contains
                bool exists = false;
                for (int j = 0; j < count; j++)
                {
                    if (targetBuffer[j] == target)
                    {
                        exists = true;
                        break;
                    }
                }

                if (!exists && count < targetBuffer.Length)
                {
                    targetBuffer[count++] = target;
                }
            }

            return count;
        }

        // =====================================================
        // DIRECTIONAL
        // =====================================================

        private static int ResolveDirectional(EffectDefinition def, EffectContext ctx)
        {
            if (Physics.Raycast(
                ctx.Origin,
                ctx.Direction,
                out RaycastHit hit,
                def.radius,
                def.layerMask,
                QueryTriggerInteraction.Ignore))
            {
                var target = ExtractTarget(hit.collider);

                if (target != null)
                {
                    targetBuffer[0] = target;
                    return 1;
                }
            }

            if (ctx is IHitPointData hitData)
                return ResolveDirectionalAtHitPoint(def, ctx, hitData);

            return 0;
        }

        private static int ResolveDirectionalAtHitPoint(
            EffectDefinition def,
            EffectContext ctx,
            IHitPointData hitData)
        {
            if (def.layerMask.value == 0)
                return 0;

            Vector3 hitPoint = hitData.HitPoint;
            float maxDistance = def.radius + 0.35f;
            if ((hitPoint - ctx.Origin).sqrMagnitude > maxDistance * maxDistance)
                return 0;

            const float probeRadius = 0.2f;
            int hits = Physics.OverlapSphereNonAlloc(
                hitPoint,
                probeRadius,
                colliderBuffer,
                def.layerMask,
                QueryTriggerInteraction.Ignore);

            for (int i = 0; i < hits; i++)
            {
                var target = ExtractTarget(colliderBuffer[i]);
                if (target == null)
                    continue;

                targetBuffer[0] = target;
                return 1;
            }

            return 0;
        }

        // =====================================================
        // FILTERS
        // =====================================================

        private static int ApplyOwnershipFilter(
            IBuffTarget[] input,
            IBuffTarget[] output,
            int count,
            EffectDefinition def,
            EffectContext ctx)
        {
            if (def.ownership == OwnershipFilter.Any)
                return Copy(input, output, count);

            int outCount = 0;

            for (int i = 0; i < count; i++)
            {
                var t = input[i];
                if (t == null)
                    continue;

                var owner = t.OwnerSource;
                var source = ctx.Source;

                if (owner == null || source == null)
                    continue;

                if ((def.ownership == OwnershipFilter.SameOwner && owner == source) ||
                    (def.ownership == OwnershipFilter.DifferentOwner && owner != source))
                {
                    output[outCount++] = t;
                }
            }

            return outCount;
        }

        private static int ApplyConeFilter(
            IBuffTarget[] input,
            IBuffTarget[] output,
            int count,
            EffectDefinition def,
            EffectContext ctx)
        {
            Vector3 origin = ctx.Origin;
            Vector3 forward = ctx.Direction.normalized;

            float halfAngle = def.coneAngle * 0.5f;

            int outCount = 0;

            for (int i = 0; i < count; i++)
            {
                var t = input[i];
                if (t == null)
                    continue;

                var tr = t.Transform;
                if (tr == null)
                    continue;

                Vector3 toTarget = tr.position - origin;
                float dist = toTarget.magnitude;

                if (dist < 2f)
                {
                    output[outCount++] = t;
                    continue;
                }

                float angle = Vector3.Angle(forward, toTarget.normalized);

                if (angle <= halfAngle)
                {
                    output[outCount++] = t;
                }
            }

            return outCount;
        }

        private static IBuffTarget SelectClosest(
            IBuffTarget[] targets,
            int count,
            EffectContext ctx)
        {
            IBuffTarget best = null;
            float bestDist = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                var t = targets[i];
                if (t == null)
                    continue;

                var go = t.BuffSystem?.gameObject;
                if (go == null)
                    continue;

                float dist = (go.transform.position - ctx.Origin).sqrMagnitude;

                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = t;
                }
            }

            return best;
        }

        private static int Copy(
            IBuffTarget[] src,
            IBuffTarget[] dst,
            int count)
        {
            Array.Copy(src, dst, count);
            return count;
        }

        private static IBuffTarget ExtractTarget(Collider collider)
        {
            if (collider == null)
                return null;

            return collider.GetComponentInParent<StatsBuffTarget>() as IBuffTarget
                ?? collider.GetComponentInParent<ResourceNodeNetwork>() as IBuffTarget;
        }
    }
}
