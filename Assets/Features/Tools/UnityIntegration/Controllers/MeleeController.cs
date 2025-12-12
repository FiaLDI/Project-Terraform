using UnityEngine;
using Features.Items.Domain;
using Features.Combat.Domain;
using Features.Equipment.Domain;
using Features.Items.UnityIntegration;

public class MeleeController : MonoBehaviour, IUsable
{
    private Camera cam;
    private ItemInstance instance;

    private float damage;
    private float range;

    // ======================================================
    // INITIALIZE (called by EquipmentManager)
    // ======================================================

    public void Initialize(Camera camera)
    {
        cam = camera;

        instance = GetComponent<ItemRuntimeHolder>()?.Instance;
        if (instance == null)
        {
            Debug.LogError("[MeleeController] ItemInstance not found");
            enabled = false;
            return;
        }

        var cfg = instance.itemDefinition.weaponConfig;
        if (cfg == null)
        {
            Debug.LogError("[MeleeController] WeaponConfig missing");
            enabled = false;
            return;
        }

        damage = cfg.meleeDamage;
        range = cfg.meleeRange;
    }

    // ======================================================
    // IUsable
    // ======================================================

    public void OnUsePrimary_Start() => DoHit();
    public void OnUsePrimary_Hold() { }
    public void OnUsePrimary_Stop() { }

    public void OnUseSecondary_Start() { }
    public void OnUseSecondary_Hold() { }
    public void OnUseSecondary_Stop() { }

    // ======================================================
    // MELEE HIT
    // ======================================================

    private void DoHit()
    {
        if (cam == null)
            return;

        if (!Physics.Raycast(
                cam.transform.position,
                cam.transform.forward,
                out RaycastHit hit,
                range))
            return;

        if (hit.collider.TryGetComponent<IDamageable>(out var damageable))
        {
            damageable.TakeDamage(damage, DamageType.Melee);
        }

#if UNITY_EDITOR
        Debug.DrawLine(
            cam.transform.position,
            cam.transform.position + cam.transform.forward * range,
            Color.red,
            0.15f);
#endif
    }
}
