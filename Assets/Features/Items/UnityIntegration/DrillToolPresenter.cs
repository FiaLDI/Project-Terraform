using UnityEngine;
using Features.Resources.UnityIntegration;
using Features.Combat.Domain;

public class DrillToolPresenter : MonoBehaviour, IUsable, IStatItem
{
    private Camera cam;
    private bool drilling;

    private float miningSpeed;
    private float damage;
    private float range;

    private float baseMiningSpeed = 10f;
    private float baseDamage = 5f;
    private float baseRange = 3f;

    private float toolMultiplier = 1f;

    public void Initialize(Camera playerCamera)
    {
        cam = playerCamera;

        miningSpeed = baseMiningSpeed;
        damage = baseDamage;
        range = baseRange;
    }

    public void ApplyRuntimeStats(ItemRuntimeStats stats)
    {
        miningSpeed = baseMiningSpeed + stats[ItemStatType.MiningSpeed];
        damage      = baseDamage      + stats[ItemStatType.Damage];
        range       = baseRange       + stats[ItemStatType.Range];
    }

    public void SetMiningMultiplier(float mult)
    {
        toolMultiplier = mult;
    }

    // --------------------------
    // PRIMARY USE
    // --------------------------
    public void OnUsePrimary_Start() => drilling = true;
    public void OnUsePrimary_Hold()  => drilling = true;
    public void OnUsePrimary_Stop()  => drilling = false;

    // --------------------------
    // SECONDARY USE (same behavior, or later — alt-mode)
    // --------------------------
    public void OnUseSecondary_Start() => drilling = true;
    public void OnUseSecondary_Hold()  => drilling = true;
    public void OnUseSecondary_Stop()  => drilling = false;

    private void Update()
    {
        if (drilling)
            TryDrill();
    }

    private void TryDrill()
    {
        if (cam == null) return;

        if (!Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, range))
            return;

        float finalMining = miningSpeed * toolMultiplier * Time.deltaTime;

        // --- mining ---
        var nodePresenter = hit.collider.GetComponentInParent<ResourceNodePresenter>();
        if (nodePresenter != null)
        {
            nodePresenter.ApplyMining(finalMining);
            return;
        }

        // --- fallback: damage ---
        var dmg = hit.collider.GetComponentInParent<IDamageable>();
        if (dmg != null)
        {
            float dmgValue = damage * toolMultiplier * Time.deltaTime;
            dmg.TakeDamage(dmgValue, DamageType.Mining);
        }
    }
}
