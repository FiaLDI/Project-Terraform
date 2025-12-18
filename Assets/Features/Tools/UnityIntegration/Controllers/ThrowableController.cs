using UnityEngine;
using Features.Items.Domain;
using Features.Equipment.Domain;
using Features.Inventory;

public class ThrowableController : MonoBehaviour, IUsable
{
    private Camera cam;
    private ItemInstance instance;
    private IInventoryContext inventory;

    private GameObject projectilePrefab;
    private float throwForce;

    // ======================================================
    // SETUP (from EquipmentManager)
    // ======================================================

    public ThrowableController Setup(ItemInstance inst)
    {
        instance = inst;
        return this;
    }

    public void Init(IInventoryContext inventory)
    {
        this.inventory = inventory;
    }

    // ======================================================
    // INITIALIZE
    // ======================================================

    public void Initialize(Camera camera)
    {
        cam = camera;

        var cfg = instance.itemDefinition.throwableConfig;
        projectilePrefab = cfg.projectilePrefab;
        throwForce = cfg.baseThrowForce;
    }

    // ======================================================
    // IUsable
    // ======================================================

    public void OnUsePrimary_Start()
    {
        Throw();
    }

    public void OnUsePrimary_Hold() { }
    public void OnUsePrimary_Stop() { }

    public void OnUseSecondary_Start() { }
    public void OnUseSecondary_Hold() { }
    public void OnUseSecondary_Stop() { }

    // ======================================================
    // THROW
    // ======================================================

    private void Throw()
    {
        if (projectilePrefab == null || inventory == null)
            return;

        // списываем предмет
        if (!inventory.Service.TryRemove(instance.itemDefinition, 1))
            return;

        Vector3 spawnPos = cam.transform.position + cam.transform.forward * 0.3f;
        GameObject proj = Instantiate(
            projectilePrefab,
            spawnPos,
            cam.transform.rotation
        );

        if (proj.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.useGravity = true;
            rb.AddForce(
                cam.transform.forward * throwForce,
                ForceMode.VelocityChange
            );
        }
    }
}
