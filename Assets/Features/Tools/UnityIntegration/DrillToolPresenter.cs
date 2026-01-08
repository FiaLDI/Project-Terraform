using UnityEngine;
using Features.Items.Domain;
using Features.Tools.Application;
using Features.Tools.Domain;
using Features.Resources.UnityIntegration;
using Features.Combat.Domain;
using Features.Equipment.Domain;
using Features.Items.UnityIntegration;
using FishNet;

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
            enabled = false;
            return;
        }

        toolService = new ToolService();
        toolService.Initialize(inst);

        stats = toolService.stats;
        enabled = true;
    }

    public void OnUsePrimary_Start() => drilling = true;
    public void OnUsePrimary_Hold()  => drilling = true;
    public void OnUsePrimary_Stop()  => drilling = false;

    public void OnUseSecondary_Start() => drilling = true;
    public void OnUseSecondary_Hold()  => drilling = true;
    public void OnUseSecondary_Stop()  => drilling = false;

    private void Update()
    {
        if (!drilling || stats == null)
        {
            fx?.Stop();
            heat = Mathf.Max(0, heat - Time.deltaTime * 1.5f);
            fx?.SetOverheat(false);
            return;
        }

        DrillTick();
    }

    private bool TryGetUseRay(out Ray ray)
    {
        // owner client
        if (cam != null)
        {
            ray = new Ray(cam.transform.position, cam.transform.forward);
            return true;
        }

        // server (нет камеры)
        var adapter = GetComponentInParent<PlayerUsageNetAdapter>();
        if (adapter != null && adapter.TryGetServerAim(out ray))
            return true;

        ray = default;
        return false;
    }

    private void DrillTick()
    {
        if (!TryGetUseRay(out var ray))
        {
            fx?.Stop();
            return;
        }

        float range = stats[ToolStat.Range];

        if (!Physics.Raycast(ray.origin, ray.direction, out var hit, range))
        {
            fx?.Stop();
            return;
        }

        fx?.Play(hit.point, hit.normal);

        heat += Time.deltaTime;
        fx?.SetOverheat(heat > heatMax);
        float mining = stats[ToolStat.MiningSpeed] * toolMultiplier * Time.deltaTime;
        float dmg    = stats[ToolStat.Damage]      * toolMultiplier * Time.deltaTime;

        var nodeNet = hit.collider.GetComponent<ResourceNodeNetwork>();
        if (nodeNet != null)
        {
            nodeNet.Mine_Server(mining, 1f);
            return;
        }

        if (InstanceFinder.IsServerStarted)
        {
            var node = hit.collider.GetComponent<ResourceNodePresenter>();
            if (node != null)
            {
                node.ApplyMining(mining);
                return;
            }

            var dmgTarget = hit.collider.GetComponentInParent<IDamageable>();
            if (dmgTarget != null)
            {
                dmgTarget.TakeDamage(dmg, DamageType.Mining);
            }
        }
    }
}
