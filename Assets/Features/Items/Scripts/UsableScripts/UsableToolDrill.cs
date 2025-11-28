using UnityEngine;

public class UsableToolDrill : MonoBehaviour, IUsable, IStatItem
{
    private Camera cam;
    private bool drilling;

    // Runtime stats
    private float miningSpeed;
    private float damage;
    private float range;

    public void Initialize(Camera playerCamera)
    {
        cam = playerCamera;
    }

    public void ApplyRuntimeStats(ItemRuntimeStats stats)
    {
        miningSpeed = stats[ItemStatType.MiningSpeed];
        damage = stats[ItemStatType.Damage];
        range = stats[ItemStatType.Range];
    }

    public void OnUsePrimary_Start() => drilling = true;
    public void OnUsePrimary_Hold() => drilling = true;
    public void OnUsePrimary_Stop() => drilling = false;

    public void OnUseSecondary_Start() => drilling = true;
    public void OnUseSecondary_Hold() => drilling = true;
    public void OnUseSecondary_Stop() => drilling = false;

    private void Update()
    {
        if (drilling)
            TryDrill();
    }

    private void TryDrill()
    {
        if (cam == null) return;

        Ray ray = AimRay.Create(cam);

        if (!Physics.Raycast(ray, out RaycastHit hit, range))
            return;

        // Mining
        if (hit.collider.TryGetComponent<IMineable>(out var mine))
        {
            float dps = miningSpeed * Time.deltaTime;
            mine.Mine(dps, null);
            return;
        }

        // Damage
        if (hit.collider.TryGetComponent<IDamageable>(out var dmg))
        {
            float dmgAmount = damage * Time.deltaTime;
            dmg.TakeDamage(dmgAmount, DamageType.Mining);
        }
    }
}
