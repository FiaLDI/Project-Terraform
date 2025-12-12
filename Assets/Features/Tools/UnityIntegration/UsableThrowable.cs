using Features.Equipment.Domain;
using Features.Items.Domain;
using UnityEngine;
using Features.Player;
using Features.Items.UnityIntegration;

public class UsableThrowable : MonoBehaviour, IUsable
{
    [SerializeField] private ThrowableAsset throwableAsset;
    [SerializeField] private float throwForce = 20f;

    private Camera playerCamera;
    private Transform spawnPoint;
    private ItemInstance instance;

    public void Initialize(Camera cam)
    {
        playerCamera = cam;
        instance = GetComponent<ItemRuntimeHolder>()?.Instance;

        if (instance == null)
            Debug.LogError("[UsableThrowable] ItemInstance not found!");
    }

    public void SetSpawnPoint(Transform point)
    {
        spawnPoint = point;
    }

    public void OnUsePrimary_Start()
    {
        if (instance == null)
            return;

        var inventory = LocalPlayerContext.Inventory;
        if (inventory == null)
            return;

        if (!inventory.Service.TryRemove(instance.itemDefinition, 1))
            return;

        ThrowProjectile();
    }

    private void ThrowProjectile()
    {
        if (throwableAsset?.projectilePrefab == null)
            return;

        Vector3 pos = spawnPoint != null ? spawnPoint.position : transform.position;
        Quaternion rot = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;

        var obj = Instantiate(throwableAsset.projectilePrefab, pos, rot);

        if (obj.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.useGravity = true;
            rb.AddForce(
                playerCamera.transform.forward * throwForce,
                ForceMode.VelocityChange
            );
        }
    }

    public void OnUsePrimary_Hold() { }
    public void OnUsePrimary_Stop() { }
    public void OnUseSecondary_Start() { }
    public void OnUseSecondary_Hold() { }
    public void OnUseSecondary_Stop() { }
}
