using UnityEngine;
using Features.Items.Domain;
using Features.Tools.Application;
using Features.Tools.Domain;
using Features.Equipment.Domain;

public class ScannerTool : MonoBehaviour, IUsable
{
    private Camera cam;
    private ItemInstance instance;

    private ToolService service;
    private ToolRuntimeStats stats;

    private float nextScanTime;

    // ==========================================================
    // SETUP from EquipmentManager
    // ==========================================================
    public ScannerTool Setup(ItemInstance inst)
    {
        instance = inst;
        return this;
    }

    // ==========================================================
    // INITIALIZE
    // ==========================================================
    public void Initialize(Camera camera)
    {
        cam = camera;

        service = new ToolService();
        service.Initialize(instance);

        stats = service.stats;
    }

    // ==========================================================
    // IUsable
    // ==========================================================
    public void OnUsePrimary_Start() => TryScan();
    public void OnUsePrimary_Hold() => TryScan();
    public void OnUsePrimary_Stop() { }

    public void OnUseSecondary_Start() { }
    public void OnUseSecondary_Hold() { }
    public void OnUseSecondary_Stop() { }

    // ==========================================================
    // CORE LOGIC
    // ==========================================================
    private void TryScan()
    {
        float cooldown = Mathf.Max(0.01f, stats[ToolStat.Cooldown]);
        if (Time.time < nextScanTime)
            return;

        nextScanTime = Time.time + cooldown;

        float range = stats[ToolStat.ScanRange];
        if (range <= 0f)
            return;

        Collider[] objects = Physics.OverlapSphere(
            cam.transform.position,
            range
        );

        float strength = stats[ToolStat.ScanSpeed];

        foreach (var col in objects)
        {
            if (col.TryGetComponent<IScannable>(out var scan))
            {
                scan.OnScanned(strength);
            }
        }

#if UNITY_EDITOR
        Debug.DrawRay(
            cam.transform.position,
            cam.transform.forward * range,
            Color.cyan,
            0.3f
        );
#endif
    }
}
