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
        if (cam == null) return;

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);

        if (!Physics.Raycast(ray, out RaycastHit hit, range))
            return;

        // MINING
        if (hit.collider.TryGetComponent<IMineable>(out var mine))
        {
            float dps = miningSpeed * Time.deltaTime;
            mine.Mine(dps, null);
            return;
        }

        // DAMAGE
        if (hit.collider.TryGetComponent<IDamageable>(out var dmg))
        {
            float dmgAmount = damage * Time.deltaTime;
            dmg.TakeDamage(dmgAmount, DamageType.Mining);
        }
    }
}
