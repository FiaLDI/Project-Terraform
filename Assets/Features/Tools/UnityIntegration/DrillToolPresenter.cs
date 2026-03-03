using Features.Buffs.Domain;
using Features.Camera.UnityIntegration;
using Features.Effects.Application;
using Features.Effects.Domain;
using Features.Equipment.Domain;
using Features.Stats.Domain;
using Features.Tools.Application;
using Features.Tools.Data;
using Features.Tools.Domain;
using FishNet;
using UnityEngine;

public sealed class DrillToolPresenter : MonoBehaviour, IUsable, IServerUsable
{
    [SerializeField] private DrillToolFX fx;
    [SerializeField] private ToolConfig toolDefinition; // <-- База инструмента

    private IBuffSource source;
    private IStatsFacade stats;
    private PlayerUsageNetAdapter usage;

    private EffectDefinition startEffect;
    private EffectDefinition stopEffect;

    private ToolEffectPipeline pipeline;
    private ToolRuntimeStats runtimeStats;

    // ======================================================
    // INIT
    // ======================================================

    public void Initialize(Camera camera)
    {
        source = GetComponentInParent<IBuffSource>();
        usage  = GetComponentInParent<PlayerUsageNetAdapter>();

        var provider = GetComponentInParent<IBuffTarget>();
        if (provider == null)
        {
            Debug.LogError("[Drill] IBuffTarget not found", this);
            return;
        }

        stats = provider.GetServerStats();

        if (InstanceFinder.IsServer)
        {
            BuildEffects();     // базовые эффекты инструмента
            BuildPipeline();    // runtime статы + pipeline
        }
    }

    // ======================================================
    // EFFECT BUILD (БАЗА ИНСТРУМЕНТА)
    // ======================================================

    private void BuildEffects()
    {
        startEffect = new EffectDefinition
        {
            type = EffectType.Continuous,
            tickInterval = 0.1f,
            childEffects = new[]
            {
                new EffectDefinition
                {
                    type = EffectType.MineNetworkResource,
                    targetMode = TargetMode.Directional,
                    radius = toolDefinition.baseRange,
                    layerMask = LayerMask.GetMask("Resource"),
                    value = toolDefinition.baseMiningSpeed
                },
                new EffectDefinition
                {
                    type = EffectType.DealDamage,
                    targetMode = TargetMode.Directional,
                    radius = toolDefinition.baseRange,
                    layerMask = LayerMask.GetMask("Enemy"),
                    value = toolDefinition.baseDamage
                }
            }
        };

        stopEffect = new EffectDefinition
        {
            type = EffectType.StopContinuous
        };
    }

    // ======================================================
    // RUNTIME STATS (ПРЕДМЕТ + ПЕРСОНАЖ)
    // ======================================================

    private ToolRuntimeStats BuildRuntimeStats()
    {
        var runtime = new ToolRuntimeStats();

        // --- БАЗА ИНСТРУМЕНТА ---
        runtime.Add(ToolStat.MiningSpeed, toolDefinition.baseMiningSpeed);
        runtime.Add(ToolStat.Range, toolDefinition.baseRange);
        runtime.Add(ToolStat.Damage, toolDefinition.baseDamage);

        // --- БОНУСЫ ПЕРСОНАЖА ---
        runtime.Add(ToolStat.MiningSpeed, stats.Mining.MiningPower);
        runtime.Add(ToolStat.Range, stats.Combat.Range);
        runtime.Add(ToolStat.Damage, stats.Combat.DamageMultiplier);

        return runtime;
    }

    private void BuildPipeline()
    {
        runtimeStats = BuildRuntimeStats();

        pipeline = new ToolEffectPipeline(
            source,
            runtimeStats,
            new[] { startEffect }
        );
    }

    // ======================================================
    // CLIENT INPUT (VFX ONLY)
    // ======================================================

    public void OnUsePrimary_Start()  => TryPlayImpactFx();
    public void OnUsePrimary_Stop()   => fx?.Stop();
    public void OnUsePrimary_Hold()   => TryPlayImpactFx();

    public void OnUseSecondary_Start() { }
    public void OnUseSecondary_Hold()  { }
    public void OnUseSecondary_Stop()  { }

    // ======================================================
    // SERVER AUTHORITATIVE
    // ======================================================

    public void ServerPrimaryStart()
    {
        if (!InstanceFinder.IsServer || pipeline == null)
            return;

        if (!TryGetServerAim(out Ray ray))
            return;

        pipeline.Execute(ray.origin, ray.direction);
    }

    public void ServerPrimaryHold()
    {
        if (!InstanceFinder.IsServer || pipeline == null)
            return;

        if (!TryGetServerAim(out Ray ray))
            return;

        pipeline.Execute(ray.origin, ray.direction);
    }

    public void ServerPrimaryStop()
    {
        if (!InstanceFinder.IsServer)
            return;

        EffectExecutor.Instance.Execute(
            stopEffect,
            new EffectContext(source, null, transform.position, transform.forward)
        );
    }

    public void ServerSecondaryStart() { }
    public void ServerSecondaryStop()  { }
    public void ServerSecondaryHold()  { }
    public void ServerReload()         { }

    // ======================================================
    // AIM (SERVER)
    // ======================================================

    private bool TryGetServerAim(out Ray ray)
    {
        ray = default;

        if (usage == null)
            return false;

        return usage.TryGetServerAim(out ray);
    }

    // ======================================================
    // CLIENT FX
    // ======================================================

    private void TryPlayImpactFx()
    {
        var cam = CameraRegistry.Instance?.CurrentCamera;

        if (cam == null || stats == null)
            return;

        float range = stats.Combat.Range;

        if (Physics.Raycast(
                cam.transform.position,
                cam.transform.forward,
                out RaycastHit hit,
                range))
        {
            fx?.Play(
                hit.point + hit.normal * 0.02f,
                hit.normal
            );
        }
    }
}