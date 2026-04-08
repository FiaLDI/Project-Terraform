using Features.Buffs.Domain;
using Features.Effects.Domain;
using UnityEngine;

public class HitEffectContext : EffectContext, IHitPointData
{
    public Vector3 HitPoint { get; private set; }
    public Vector3 HitNormal { get; private set; }
    public HitEffectContext() { }

    public HitEffectContext(
        IBuffSource source,
        IBuffTarget[] targets,
        Vector3 origin,
        Vector3 direction,
        Vector3 hitPoint,
        Vector3 hitNormal
    ) : base(source, targets, origin, direction)
    {
        HitPoint = hitPoint;
        HitNormal = hitNormal;
    }

    public void UpdateHit(Vector3 point, Vector3 normal)
    {
        HitPoint = point;
        HitNormal = normal;
    }
}

