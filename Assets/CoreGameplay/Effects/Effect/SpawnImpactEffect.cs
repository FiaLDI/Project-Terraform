using Features.Effects.Domain;
using FishNet;

public sealed class SpawnImpactEffect : IEffect
{
    private readonly string _fxId;

    public SpawnImpactEffect(string fxId)
    {
        _fxId = fxId;
    }

    public void Apply(EffectContext context)
    {
        if (!InstanceFinder.IsServer)
            return;

        if (string.IsNullOrEmpty(_fxId))
            return;

        if (context is not IHitPointData hitData)
            return;

        ImpactFxDispatcher.Instance.ServerSpawn(
            hitData.HitPoint,
            hitData.HitNormal,
            _fxId
        );
    }
}
