using Features.Equipment.Domain;
using Features.Items.Domain;
using Features.Resources.Domain;
using Features.Tools.Application;
using Features.Tools.Domain;
using UnityEngine;

public class ToolController : MonoBehaviour, IUsable
{
    private Camera cam;
    private ToolService service;
    private ToolRuntimeStats stats;
    private ItemInstance instance;

    private float nextUseTime;

    // ==========================================================
    // SETUP from EquipmentManager
    // ==========================================================
    public ToolController Setup(ItemInstance inst)
    {
        instance = inst;
        return this;
    }

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

    public void OnUsePrimary_Start() => TryUse();
    public void OnUsePrimary_Hold()  => TryUse();
    public void OnUsePrimary_Stop()  { }

    public void OnUseSecondary_Start() { }
    public void OnUseSecondary_Hold()  { }
    public void OnUseSecondary_Stop()  { }

    // ==========================================================
    // CORE LOGIC
    // ==========================================================

    private void TryUse()
    {
        // cooldown
        float cooldown = Mathf.Max(0.01f, stats[ToolStat.Cooldown]);
        if (Time.time < nextUseTime)
            return;

        nextUseTime = Time.time + cooldown;

        PerformMining();
        service.ApplyUseEffects(instance);
    }

    private void PerformMining()
    {
        float range = stats[ToolStat.Range];
        if (range <= 0f)
            return;

        if (!Physics.Raycast(
                cam.transform.position,
                cam.transform.forward,
                out var hit,
                range))
            return;

        if (hit.collider.TryGetComponent(out IMineable target))
        {
            float miningSpeed = Mathf.Max(0f, stats[ToolStat.MiningSpeed]);
            float damage = Mathf.Max(0f, stats[ToolStat.Damage]);

            target.Mine(miningSpeed * Time.deltaTime);
        }
    }

}
