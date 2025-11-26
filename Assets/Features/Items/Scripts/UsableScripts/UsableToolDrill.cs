using UnityEngine;

public class UsableToolDrill : MonoBehaviour, IUsable
{
    public float damagePerSecond = 10f;
    public float range = 3f;
    public LayerMask hitMask;

    private Camera cam;
    private bool drilling = false;

    public void Initialize(Camera playerCamera)
    {
        cam = playerCamera;
    }

    // PRIMARY (ЛКМ)
    public void OnUsePrimary_Start()  => drilling = true;
    public void OnUsePrimary_Hold()   => drilling = true;
    public void OnUsePrimary_Stop()   => drilling = false;

    // SECONDARY (ПКМ)
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
        Ray ray = AimRay.Create(cam);

        if (!Physics.Raycast(ray, out RaycastHit hit, range, hitMask))
            return;

        // 1) Майнинг
        var mine = hit.collider.GetComponentInParent<IMineable>();
        if (mine != null)
        {
            Tool tool = EquipmentManager.instance.GetCurrentEquippedItem() as Tool;
            float miningDps = damagePerSecond * Time.deltaTime * GlobalMiningSpeed.Multiplier;

            mine.Mine(miningDps, tool);
            return;
        }

        // 2) Урон
        var dmg = hit.collider.GetComponent<IDamageable>();
        if (dmg != null)
        {
            float dmgAmount = damagePerSecond * Time.deltaTime * GlobalMiningSpeed.Multiplier;
            dmg.TakeDamage(dmgAmount, DamageType.Mining);
            return;
        }
    }
}
