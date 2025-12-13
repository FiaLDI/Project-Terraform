using UnityEngine;
using Features.Items.Domain;
using Features.Tools.Application;
using Features.Tools.Domain;
using Features.Resources.UnityIntegration;
using Features.Combat.Domain;
using Features.Equipment.Domain;
using Features.Items.UnityIntegration;

public class DrillToolPresenter : MonoBehaviour, IUsable
{
    [Header("FX")]
    public DrillToolFX fx;

    private Camera cam;
    private ToolRuntimeStats stats;
    private ToolService toolService;

    private bool drilling;
    private float heat;
    private const float heatMax = 5f;

    private float toolMultiplier = 1f;

    public void Initialize(Camera camera)
    {
        cam = camera;

        var inst = GetComponent<ItemRuntimeHolder>()?.Instance;
        if (inst == null || inst.itemDefinition == null)
        {
            // НОРМАЛЬНОЕ состояние (предмет сняли / переключили)
            enabled = false;
            return;
        }

        toolService = new ToolService();
        toolService.Initialize(inst);

        stats = toolService.stats;
        enabled = true;
    }

    // ============================================================
    // IUsable
    // ============================================================

    public void OnUsePrimary_Start() => drilling = true;
    public void OnUsePrimary_Hold()  => drilling = true;
    public void OnUsePrimary_Stop()  => drilling = false;

    public void OnUseSecondary_Start() => drilling = true;
    public void OnUseSecondary_Hold()  => drilling = true;
    public void OnUseSecondary_Stop()  => drilling = false;

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

        DrillTick();
    }

    private void DrillTick()
    {
        if (!Physics.Raycast(
                cam.transform.position,
                cam.transform.forward,
                out var hit,
                stats[ToolStat.Range]))
        {
            fx?.Stop();
            return;
        }

        fx?.Play(hit.point, hit.normal);

        heat += Time.deltaTime;
        if (heat > heatMax)
            fx?.SetOverheat(true);

        float mining = stats[ToolStat.MiningSpeed] * toolMultiplier * Time.deltaTime;
        float dmg    = stats[ToolStat.Damage]      * toolMultiplier * Time.deltaTime;

        var node = FindComponentUpwards<ResourceNodePresenter>(hit.collider.transform);
        if (node != null)
        {
            node.ApplyMining(mining);
            return;
        }

        var damageable = FindComponentUpwards<IDamageable>(hit.collider.transform);
        if (damageable != null)
        {
            damageable.TakeDamage(dmg, DamageType.Mining);
            return;
        }
    }

    private T FindComponentUpwards<T>(Transform t) where T : class
    {
        while (t != null)
        {
            if (t.TryGetComponent(out T result))
                return result;
            t = t.parent;
        }
        return null;
    }
}
