using UnityEngine;
using FishNet;
using Features.Effects.Domain;
using Features.Effects.Application;
using Features.Buffs.Domain;
using Features.Stats.Domain;
using Features.Equipment.Domain;

public sealed class DrillToolPresenter : MonoBehaviour, IUsable
{
    [SerializeField] private DrillToolFX fx;

    private IBuffSource source;
    private IStatsFacade stats;

    private EffectDefinition startEffect;
    private EffectDefinition stopEffect;

    // ======================================================
    // INIT
    // ======================================================

    public void Initialize(Camera camera)
    {
        source = GetComponentInParent<IBuffSource>();

        var provider = GetComponentInParent<IBuffTarget>();
        if (provider == null)
        {
            Debug.LogError("[Drill] IStatsOwner not found", this);
            return;
        }

        stats = provider.GetServerStats();

        BuildEffects();
    }

    private void BuildEffects()
    {
        float miningPower = stats.Mining.MiningPower;
        float damageMult = stats.Combat.DamageMultiplier;
        float range = stats.Combat.Range;

        startEffect = new EffectDefinition
        {
            type = EffectType.Continuous,
            value = 0.1f, // tick interval

            childEffects = new[]
            {
                new EffectDefinition
                {
                    type = EffectType.MineNetworkResource,
                    targetMode = TargetMode.Directional,
                    radius = range,
                    layerMask = LayerMask.GetMask("Resource"),
                    value = miningPower
                },

                new EffectDefinition
                {
                    type = EffectType.DealDamage,
                    targetMode = TargetMode.Directional,
                    radius = range,
                    layerMask = LayerMask.GetMask("Enemy"),
                    value = damageMult
                }
            }
        };

        stopEffect = new EffectDefinition
        {
            type = EffectType.StopContinuous
        };
    }

    // ======================================================
    // INPUT
    // ======================================================

    public void OnUsePrimary_Start()
    {
        fx?.Play(transform.position, transform.forward);
        Execute(startEffect);
    }

    public void OnUsePrimary_Stop()
    {
        fx?.Stop();
        Execute(stopEffect);
    }

    public void OnUsePrimary_Hold() { }

    public void OnUseSecondary_Start() { }
    public void OnUseSecondary_Hold() { }
    public void OnUseSecondary_Stop() { }

    // ======================================================
    // EXECUTION
    // ======================================================

    private void Execute(EffectDefinition def)
    {
        if (!InstanceFinder.IsServer)
            return;

        if (source == null)
            return;

        var ctx = new EffectContext(
            source,
            null,
            transform.position,
            transform.forward
        );

        EffectExecutor.Instance.Execute(def, ctx);
    }
}
