using UnityEngine;
using FishNet;
using Features.Tools.Data;
using Features.Items.Domain;
using Features.Effects.Domain;
using Features.Effects.Application;
using Features.Buffs.Domain;
using Features.Equipment.Domain;

public sealed class ScannerTool : MonoBehaviour, IUsable
{
    private Camera cam;
    private ItemInstance instance;
    private ScannerConfig config;

    private IBuffSource source;
    private float nextScanTime;

    // =====================================================
    // SETUP
    // =====================================================

    public ScannerTool Setup(ItemInstance inst)
    {
        instance = inst;
        return this;
    }

    public void Initialize(Camera camera)
    {
        cam = camera;

        if (instance == null || instance.itemDefinition == null)
        {
            enabled = false;
            return;
        }

        //config = instance.itemDefinition.scannerConfig;
        if (config == null)
        {
            Debug.LogError("[ScannerTool] ScannerConfig missing", this);
            enabled = false;
            return;
        }

        source = GetComponentInParent<IBuffSource>();
        if (source == null)
        {
            Debug.LogError("[ScannerTool] IBuffSource missing", this);
            enabled = false;
            return;
        }

        enabled = true;
    }

    // =====================================================
    // INPUT
    // =====================================================

    public void OnUsePrimary_Start() => TryScan();
    public void OnUsePrimary_Hold()  => TryScan();
    public void OnUsePrimary_Stop()  { }

    public void OnUseSecondary_Start() { }
    public void OnUseSecondary_Hold()  { }
    public void OnUseSecondary_Stop()  { }

    // =====================================================
    // LOGIC (SERVER ONLY)
    // =====================================================

    private void TryScan()
    {
        if (!InstanceFinder.IsServer)
            return;

        if (Time.time < nextScanTime)
            return;
        
        

        if (config == null)
        {
            Debug.LogError("[ScannerTool] config.effects is NULL");
            return;
        }

        nextScanTime = Time.time + config.cooldown;

        Vector3 origin = cam != null
            ? cam.transform.position
            : transform.position;

        Vector3 direction = cam != null
            ? cam.transform.forward
            : transform.forward;

        var ctx = new EffectContext(
            source,
            null,           // Targets resolved inside executor
            origin,
            direction
        );

        if (EffectExecutor.Instance == null)
        {
            Debug.LogError("[ScannerTool] EffectExecutor.Instance is NULL");
            return;
        }

        foreach (var def in config.effects)
            EffectExecutor.Instance.Execute(def, ctx);
    }
}
