using UnityEngine;
using Features.Resources.UnityIntegration;
using Features.Combat.Domain;
using Features.Camera.UnityIntegration;

public class DrillToolPresenter : MonoBehaviour, IUsable, IStatItem
{
    [Header("Debug")]
    public bool debugEnabled = true;
    public bool debugDeep = true;

    private bool drilling;

    private float miningSpeed=1;
    private float damage=1;
    private float range=3f;

    public DrillToolFX fx;

    private float heat;
    private const float heatMax = 5f;

    private float toolMultiplier = 1f;

    private string lastDebugMessage = "";
    private float lastDebugTime = 0f;


    // ============================================================
    // IUsable
    // ============================================================

    public void Initialize(Camera cam)
    {
        Log($"Initialize() called. cam = {(cam ? cam.name : "NULL")}");
    }

    public void OnUsePrimary_Start()
    {
        drilling = true;
        Log("Primary_Start");
    }

    public void OnUsePrimary_Hold()
    {
        drilling = true;
    }

    public void OnUsePrimary_Stop()
    {
        drilling = false;
        Log("Primary_Stop");
    }

    public void OnUseSecondary_Start()
    {
        drilling = true;
        Log("Secondary_Start");
    }

    public void OnUseSecondary_Hold()
    {
        drilling = true;
    }

    public void OnUseSecondary_Stop()
    {
        drilling = false;
        Log("Secondary_Stop");
    }

    // ============================================================
    // IStatItem
    // ============================================================

    public void ApplyRuntimeStats(ItemRuntimeStats stats)
    {
        miningSpeed = 10f + stats[ItemStatType.MiningSpeed];
        damage      = 5f  + stats[ItemStatType.Damage];
        range       = 3f  + stats[ItemStatType.Range];

        Log($"ApplyStats → mining:{miningSpeed}, dmg:{damage}, range:{range}");
    }

    public void SetMiningMultiplier(float mult)
    {
        toolMultiplier = mult;
        Log($"SetMultiplier → {mult}");
    }

    // ============================================================

    private void Update()
    {
        if (!drilling)
        {
            fx?.Stop();
            heat = Mathf.Max(0, heat - Time.deltaTime * 1.5f);
            fx?.SetOverheat(false);
            return;
        }

        TryDrill();
    }


    // ============================================================
    // DRILL LOGIC
    // ============================================================

    private void TryDrill()
    {
        Ray ray = GetRay();
        RaycastHit hit;

        LogDeep($"Ray: origin={ray.origin} dir={ray.direction} range={range}");

        Debug.DrawLine(ray.origin, ray.origin + ray.direction * range, Color.red);

        DebugRayDrawer.Instance?.DrawLaserRay(ray.origin, ray.origin + ray.direction * range, 0.2f);

        if (!Physics.Raycast(ray, out hit, range))
        {
            fx?.Stop();
            Log("Raycast MISS");
            return;
        }

        Log($"HIT {hit.collider.name} at distance {hit.distance:F2}");

        fx?.Play(hit.point, hit.normal);

        // Heat
        heat += Time.deltaTime;
        if (heat > heatMax)
            fx?.SetOverheat(true);

        // -------------------------------------------------------
        // FIND ResourceNodePresenter (Unity safe search)
        // -------------------------------------------------------
        var node = FindComponentUpwards<ResourceNodePresenter>(hit.collider.transform);

        if (node != null)
        {
            float m = miningSpeed * toolMultiplier * Time.deltaTime;
            node.ApplyMining(m);
            Log($"MINING {node.name} → +{m:F2}");
            return;
        }

        // -------------------------------------------------------
        // FIND IDamageable (same robust search)
        // -------------------------------------------------------
        var dmg = FindComponentUpwards<IDamageable>(hit.collider.transform);

        if (dmg != null)
        {
            float d = damage * toolMultiplier * Time.deltaTime;
            dmg.TakeDamage(d, DamageType.Mining);
            Log($"DAMAGE {dmg} → {d:F2}");
            return;
        }

        Log("Hit object has NO mining/damage component");
    }


    // ============================================================
    // SAFE SEARCH FIX FOR NESTED PREFABS
    // ============================================================

    private T FindComponentUpwards<T>(Transform t) where T : class
    {
        while (t != null)
        {
            T c = t.GetComponent<T>();
            if (c != null)
                return c;

            t = t.parent;
        }
        return null;
    }


    // ============================================================
    // CAMERA
    // ============================================================

    private Ray GetRay()
    {
        var cam = CameraRegistry.Instance?.CurrentCamera;
        if (cam)
        {
            LogDeep($"Using camera: {cam.name} pos={cam.transform.position}");
            return cam.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        }

        Log("Camera MISSING! Using forward ray fallback");
        return new Ray(transform.position, transform.forward);
    }


    // ============================================================
    // DEBUG SYSTEM
    // ============================================================

    private void Log(string msg)
    {
        if (!debugEnabled) return;
        lastDebugTime = Time.time;
        lastDebugMessage = msg;

        Debug.Log($"<color=#ffaa00>[DRILL]</color> {msg}");
    }

    private void LogDeep(string msg)
    {
        if (!debugEnabled || !debugDeep) return;
        Debug.Log($"<color=#00ffff>[DRILL-DEBUG]</color> {msg}");
    }

    private void OnGUI()
    {
        if (!debugEnabled) return;
        if (Time.time - lastDebugTime > 1f) return;

        GUI.color = Color.yellow;
        GUI.Label(new Rect(30, 300, 900, 200), $"<b>DRILL DEBUG:</b> {lastDebugMessage}");
    }
}
