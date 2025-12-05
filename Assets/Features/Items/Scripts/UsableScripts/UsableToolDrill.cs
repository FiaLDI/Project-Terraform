using UnityEngine;
using Features.Combat.Domain;

public class UsableToolDrill : MonoBehaviour, IUsable, IStatItem
{
    private Camera cam;
    private bool drilling;

    private PlayerMiningStats miningStats;

    [Header("Base Stats")]
    [SerializeField] private float baseMiningSpeed = 10f;
    [SerializeField] private float baseDamage = 5f;
    [SerializeField] private float baseRange = 3f;

    private float miningSpeed;
    private float damage;
    private float range;

    public void Initialize(Camera playerCamera)
    {
        cam = playerCamera;

        // Получаем PlayerMiningStats от игрока
        miningStats = cam.GetComponentInParent<PlayerMiningStats>();

        if (range <= 0)
            range = baseRange;
        if (miningSpeed <= 0)
            miningSpeed = baseMiningSpeed;
        if (damage <= 0)
            damage = baseDamage;
    }

    public void ApplyRuntimeStats(ItemRuntimeStats stats)
    {
        miningSpeed = baseMiningSpeed + stats[ItemStatType.MiningSpeed];
        damage      = baseDamage      + stats[ItemStatType.Damage];
        range       = baseRange       + stats[ItemStatType.Range];
    }

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
        if (cam == null) return;

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);

        if (!Physics.Raycast(ray, out RaycastHit hit, range))
            return;

        float multiplier = miningStats != null ? miningStats.MiningMultiplier : 1f;

        var mine = hit.collider.GetComponentInParent<IMineable>();
        if (mine != null)
        {
            float dps = miningSpeed * multiplier * Time.deltaTime;
            mine.Mine(dps, null);
            return;
        }

        var dmg = hit.collider.GetComponentInParent<IDamageable>();
        if (dmg != null)
        {
            float dmgValue = damage * multiplier * Time.deltaTime;
            dmg.TakeDamage(dmgValue, DamageType.Mining);
            return;
        }
    }
}
