using UnityEngine;

public class UsableThrowable : MonoBehaviour, IUsable
{
    [SerializeField] private ThrowableAsset throwableAsset;

    private Transform spawnPoint;
    private Camera playerCamera;
    private Item itemData;
    [SerializeField] private float throwForce = 20f;
    public void SetSpawnPoint(Transform point)
    {
        spawnPoint = point;
    }

    public void Initialize(Camera cam)
    {
        playerCamera = cam;
    }

    private void Start()
    {
        itemData = GetComponent<ItemObject>().itemData;
    }

    public void OnUsePrimary_Start()
    {
        if (!InventoryManager.instance.ConsumeItem(itemData, 1))
            return;

        if (throwableAsset == null || throwableAsset.projectilePrefab == null)
        {
            Debug.LogError("UsableThrowable: Нет ссылок на asset или prefab!");
            return;
        }

        // 1) Создаём снаряд
        GameObject instance = Instantiate(
            throwableAsset.projectilePrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        if (instance.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.AddForce(spawnPoint.forward * 20f, ForceMode.VelocityChange);
        }

    }

    public void OnUsePrimary_Hold() { }
    public void OnUsePrimary_Stop() { }
}
