using UnityEngine;
using Features.Interaction.Domain;
using Features.Resources.UnityIntegration;
using Features.Combat.Domain;
using Features.Interaction.UnityIntegration;

public class DrillToolPresenter : MonoBehaviour, IUsable, IStatItem
{
    private bool drilling;

    private float miningSpeed;
    private float damage;
    private float range;

    private float baseMiningSpeed = 10f;
    private float baseDamage = 5f;
    private float baseRange = 3f;

    private float toolMultiplier = 1f;

    public void Initialize(Camera _) { }

    public void ApplyRuntimeStats(ItemRuntimeStats stats)
    {
        miningSpeed = baseMiningSpeed + stats[ItemStatType.MiningSpeed];
        damage      = baseDamage      + stats[ItemStatType.Damage];
        range       = baseRange       + stats[ItemStatType.Range];
    }

    public void SetMiningMultiplier(float mult) => toolMultiplier = mult;

    public void OnUsePrimary_Start() => drilling = true;
    public void OnUsePrimary_Hold()  => drilling = true;
    public void OnUsePrimary_Stop()  => drilling = false;

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
        var hit = InteractionServiceProvider.Ray.Raycast();
        if (!hit.Hit) return;

        var collider = hit.HitInfo.collider;

        // --- MINING ---
        float finalMining = miningSpeed * toolMultiplier * Time.deltaTime;

        var node = collider.GetComponentInParent<ResourceNodePresenter>();
        if (node != null)
        {
            node.ApplyMining(finalMining);
            return;
        }

        // --- DAMAGE ---
        var dmg = collider.GetComponentInParent<IDamageable>();
        if (dmg != null)
            dmg.TakeDamage(damage * toolMultiplier * Time.deltaTime, DamageType.Mining);
    }
}
