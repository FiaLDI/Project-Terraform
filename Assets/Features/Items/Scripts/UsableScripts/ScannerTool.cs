using UnityEngine;

public class ScannerTool : MonoBehaviour, IUsable, IStatItem
{
    private Camera cam;

    // Runtime stats
    private float scanRange;
    private float scanSpeed;
    private float cooldown;
    
    private float nextScanTime = 0f;

    public void Initialize(Camera playerCamera)
    {
        cam = playerCamera;
    }

    public void ApplyRuntimeStats(ItemRuntimeStats stats)
    {
        scanRange = stats[ItemStatType.ScanRange];
        scanSpeed = stats[ItemStatType.ScanSpeed];
        cooldown = stats[ItemStatType.Cooldown];
    }

    public void OnUsePrimary_Start() => TryScan();
    public void OnUsePrimary_Hold() => TryScan();
    public void OnUsePrimary_Stop() { }

    public void OnUseSecondary_Start() { }
    public void OnUseSecondary_Hold() { }
    public void OnUseSecondary_Stop() { }

    private void TryScan()
    {
        if (Time.time < nextScanTime) return;
        nextScanTime = Time.time + cooldown;

        Collider[] detected = Physics.OverlapSphere(cam.transform.position, scanRange);

        foreach (var col in detected)
        {
            if (col.TryGetComponent<IScannable>(out var scan))
            {
                scan.OnScanned(scanSpeed);
            }
        }

        Debug.DrawLine(cam.transform.position, cam.transform.position + cam.transform.forward * scanRange, Color.cyan, 1f);
    }
}
