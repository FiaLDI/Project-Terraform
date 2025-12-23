using UnityEngine;
using Features.Items.Domain;
using Features.Tools.Application;
using Features.Tools.Domain;
using Features.Equipment.Domain;

public class ScannerTool : MonoBehaviour, IUsable, ILocalOnlyUsable
{
    private Camera cam;
    private ItemInstance instance;
    private ToolService service;
    private ToolRuntimeStats stats;
    private float nextScanTime;

    public ScannerTool Setup(ItemInstance inst)
    {
        instance = inst;
        return this;
    }

    public void Initialize(Camera camera)
    {
        cam = camera;
        if (cam == null)
        {
            enabled = false;
            return;
        }

        service = new ToolService();
        service.Initialize(instance);
        stats = service.stats;
        enabled = true;
    }

    public void OnUsePrimary_Start() => TryScan();
    public void OnUsePrimary_Hold()  => TryScan();
    public void OnUsePrimary_Stop()  { }

    public void OnUseSecondary_Start() { }
    public void OnUseSecondary_Hold()  { }
    public void OnUseSecondary_Stop()  { }

    private void TryScan()
    {
        if (cam == null || stats == null) return;

        float cooldown = Mathf.Max(0.01f, stats[ToolStat.Cooldown]);
        if (Time.time < nextScanTime) return;

        nextScanTime = Time.time + cooldown;

        float range = stats[ToolStat.ScanRange];
        if (range <= 0f) return;

        var objects = Physics.OverlapSphere(cam.transform.position, range);
        float strength = stats[ToolStat.ScanSpeed];

        foreach (var col in objects)
            if (col.TryGetComponent<IScannable>(out var scan))
                scan.OnScanned(strength);
    }
}
