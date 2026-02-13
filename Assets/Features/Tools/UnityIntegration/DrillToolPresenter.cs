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
            Debug.LogError("[Drill] IBuffTarget not found", this);
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
            value = 0.1f,
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
        TryPlayImpactFx();
        Execute(startEffect);
    }

    public void OnUsePrimary_Stop()
    {
        fx?.Stop();
        Execute(stopEffect);
    }

    public void OnUsePrimary_Hold()
    {
        TryPlayImpactFx();
    }

    public void OnUseSecondary_Start() { }
    public void OnUseSecondary_Hold() { }
    public void OnUseSecondary_Stop() { }

    // ======================================================
    // IMPACT FX
    // ======================================================

    private void TryPlayImpactFx()
    {
        if (stats == null) return;

        float range = stats.Combat.Range;

        Debug.DrawRay(transform.position, transform.forward * range, Color.red);

        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, range))
        {
            Debug.Log("HIT: " + hit.collider.name);

            fx?.Play(
                hit.point + hit.normal * 0.02f,
                hit.normal
            );
        }
        else
        {
            Debug.Log("NO HIT");
        }
    }



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
