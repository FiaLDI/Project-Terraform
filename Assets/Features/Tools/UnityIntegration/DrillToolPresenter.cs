using UnityEngine;
using Features.Effects.Domain;
using Features.Effects.Application;
using Features.Buffs.Domain;
using Features.Items.Domain;
using Features.Tools.Application;
using Features.Tools.Domain;
using FishNet;
using Features.Equipment.Domain;
using Features.Items.UnityIntegration;

public sealed class DrillToolPresenter : MonoBehaviour, IUsable
{
    [SerializeField] private DrillToolFX fx;

    private IBuffSource source;
    private ToolRuntimeStats runtimeStats;

    private EffectDefinition startEffect;
    private EffectDefinition stopEffect;

    // ======================================================
    // INIT
    // ======================================================

    public void Initialize(Camera camera)
    {
        source = GetComponentInParent<IBuffSource>();

        var holder = GetComponent<ItemRuntimeHolder>();
        if (holder == null || holder.Instance == null)
        {
            Debug.LogError("[DrillTool] ItemRuntimeHolder missing", this);
            return;
        }

        runtimeStats = ToolStatCalculator.Calculate(holder.Instance);

        BuildEffects();
    }

    private void BuildEffects()
    {
        float damage = runtimeStats[ToolStat.Damage];
        float mining = runtimeStats[ToolStat.MiningSpeed];
        float range  = runtimeStats[ToolStat.Range];

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
                    radius = 3f,
                    layerMask = LayerMask.GetMask("Resource"),
                    value = 5f
                },

                new EffectDefinition
                {
                    type = EffectType.DealDamage,
                    targetMode = TargetMode.Directional,
                    radius = range,
                    layerMask = LayerMask.GetMask("Resource"),
                    value = damage
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

    public void OnUsePrimary_Hold() { }

    public void OnUsePrimary_Stop()
    {
        fx?.Stop();
        Execute(stopEffect);
    }

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
        {
            Debug.LogWarning("[Drill] IBuffSource missing");
            return;
        }

        if (EffectExecutor.Instance == null)
        {
            Debug.LogError("[Drill] EffectExecutor missing");
            return;
        }

        var ctx = new EffectContext(
            source,
            null,
            transform.position,
            transform.forward
        );

        EffectExecutor.Instance.Execute(def, ctx);
    }

}
