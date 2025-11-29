using UnityEngine;

public static class ResourceDropSystem
{
    public static void Drop(ResourceDropSO dropTable, Transform dropOrigin)
    {
        if (dropTable == null)
        {
            Debug.LogWarning("[DropSystem] Drop table is null.");
            return;
        }

        foreach (var drop in dropTable.drops)
        {
            if (drop == null || drop.resource == null)
                continue;

            if (Random.value > drop.chance)
                continue;

            int amount = Random.Range(drop.minAmount, drop.maxAmount + 1);

            if (drop.resource.item == null || drop.resource.item.worldPrefab == null)
            {
                Debug.LogWarning($"[DropSystem] Resource '{drop.resource.resourceName}' has no linked Item or worldPrefab!");
                continue;
            }

            GameObject prefab = drop.resource.item.worldPrefab;

            for (int i = 0; i < amount; i++)
            {
                Vector3 spawnPos = dropOrigin.position + Vector3.up * 0.75f;
                Vector3 randomOffset = (Random.insideUnitSphere * 0.4f);
                randomOffset.y = 0f;
                spawnPos += randomOffset;

                GameObject loot = GameObject.Instantiate(prefab, spawnPos, Random.rotation);

                if (loot.TryGetComponent<ItemObject>(out var io))
                {
                    Item baseItem = drop.resource.item;

                    // non-stackable → runtime клон с lvl=0,
                    // stackable → можно сам asset
                    Item runtimeItem = baseItem.isStackable
                        ? baseItem
                        : ItemFactory.CreateRuntimeItem(baseItem);

                    io.itemData = runtimeItem;
                    io.quantity = 1;
                }

                Rigidbody rb = loot.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = false;
                    rb.useGravity = true;

                    Vector3 impulse =
                        dropOrigin.forward * Random.Range(1.5f, 3f) +
                        Vector3.up * Random.Range(2f, 4f) +
                        Random.insideUnitSphere * 1.5f;

                    rb.AddForce(impulse, ForceMode.Impulse);
                    rb.AddTorque(Random.insideUnitSphere * 5f, ForceMode.Impulse);
                }
            }
        }
    }
}
