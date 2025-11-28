using UnityEngine;

public class UsableToolDrill : MonoBehaviour, IUsable, IStatItem
{
    private Camera cam;
    private bool drilling;

    // BASE STATS (если вдруг нет runtime-статов)
    [Header("Base Stats")]
    [SerializeField] private float baseMiningSpeed = 10f;
    [SerializeField] private float baseDamage = 5f;
    [SerializeField] private float baseRange = 3f;

    // FINAL STAT VALUES
    private float miningSpeed;
    private float damage;
    private float range;

    public void Initialize(Camera playerCamera)
    {
        cam = playerCamera;

        // fallback на случай если статов нет
        if (range <= 0)
            range = baseRange;
        if (miningSpeed <= 0)
            miningSpeed = baseMiningSpeed;
        if (damage <= 0)
            damage = baseDamage;
    }

    public void ApplyRuntimeStats(ItemRuntimeStats stats)
    {
        // каждая характеристика = базовая + бонус из статов
        miningSpeed = baseMiningSpeed + stats[ItemStatType.MiningSpeed];
        damage      = baseDamage      + stats[ItemStatType.Damage];
        range       = baseRange       + stats[ItemStatType.Range];

        // дебаг
        Debug.Log($"[DRILL] Stats applied → speed={miningSpeed} dmg={damage} range={range}");
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
        Debug.Log("[DRILL] TryDrill()");

        if (cam == null)
        {
            Debug.Log("[DRILL] NO CAMERA");
            return;
        }

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        Debug.DrawRay(ray.origin, ray.direction * range, Color.red);

        Debug.Log("[DRILL] Raycasting...");

        if (!Physics.Raycast(ray, out RaycastHit hit, range))
        {
            Debug.Log("[DRILL] NO HIT");
            return;
        }

        Debug.Log("[DRILL] HIT " + hit.collider.name);

        var mine = hit.collider.GetComponentInParent<IMineable>();
        if (mine != null)
        {
            Debug.Log("[DRILL] Mineable found! DPS=" + miningSpeed);
            float dps = miningSpeed * Time.deltaTime;
            mine.Mine(dps, null);
            return;
        }

        Debug.Log("[DRILL] Not mineable collider, checking damageable");

        var dmg = hit.collider.GetComponentInParent<IDamageable>();
        if (dmg != null)
        {
            Debug.Log("[DRILL] Damageable found! DMG=" + damage);
            float dmgAmount = damage * Time.deltaTime;
            dmg.TakeDamage(dmgAmount, DamageType.Mining);
            return;
        }

        Debug.Log("[DRILL] Nothing applicable hit.");
    }

}
