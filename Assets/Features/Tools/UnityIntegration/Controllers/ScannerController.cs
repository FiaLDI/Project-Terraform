using UnityEngine;
using Features.Items.Domain;
using Features.Tools.Application;
using Features.Tools.Domain;
using Features.Equipment.Domain;

public class ScannerController : MonoBehaviour, IUsable
{
    private Camera cam;
    private ItemInstance instance;

    private float range;
    private float scanSpeed;
    private float cooldown;

    private float nextScanTime;

    public ScannerController Setup(ItemInstance inst)
    {
        instance = inst;
        return this;
    }

    public void Initialize(Camera camera)
    {
        cam = camera;

        var cfg = instance.itemDefinition.scannerConfig;

        range     = cfg.baseScanRange;
        scanSpeed = cfg.baseScanSpeed;
        cooldown  = cfg.baseCooldown;
    }

    public void OnUsePrimary_Start() => Scan();
    public void OnUsePrimary_Hold()  => Scan();
    public void OnUsePrimary_Stop()  { }

    public void OnUseSecondary_Start() { }
    public void OnUseSecondary_Hold()  { }
    public void OnUseSecondary_Stop()  { }

    private void Scan()
    {
        if (Time.time < nextScanTime) return;
        nextScanTime = Time.time + cooldown;

        Collider[] hits = Physics.OverlapSphere(cam.transform.position, range);

        foreach (var c in hits)
        {
            if (c.TryGetComponent<IScannable>(out var sc))
                sc.OnScanned(scanSpeed);
        }

        Debug.DrawRay(cam.transform.position, cam.transform.forward * range, Color.cyan, 0.4f);
    }
}
