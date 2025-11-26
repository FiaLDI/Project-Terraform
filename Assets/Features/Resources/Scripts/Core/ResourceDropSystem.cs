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

            // Шанс
            if (Random.value > drop.chance)
                continue;

            // Количество
            int amount = Random.Range(drop.minAmount, drop.maxAmount + 1);

            Debug.Log($"[DropSystem] {amount} x {drop.resource.resourceName}");

            if (drop.resource.item == null || drop.resource.item.worldPrefab == null)
            {
                Debug.LogWarning($"[DropSystem] Resource '{drop.resource.resourceName}' has no linked Item or worldPrefab!");
                continue;
            }

            GameObject prefab = drop.resource.item.worldPrefab;
            if (prefab == null)
            {
                Debug.LogWarning($"[DropSystem] Resource '{drop.resource.resourceName}' has no worldPrefab!");
                continue;
            }

            // Создание предметов
            for (int i = 0; i < amount; i++)
            {
                // Позиция появления — чуть над землей
                Vector3 spawnPos = dropOrigin.position + Vector3.up * 0.75f;

                // Маленький разброс в стороны (0.4 м)
                Vector3 randomOffset = (Random.insideUnitSphere * 0.4f);
                randomOffset.y = 0f;
                spawnPos += randomOffset;

                // Создание предмета
                GameObject loot = GameObject.Instantiate(prefab, spawnPos, Random.rotation);

                // Привязка ItemObject
                if (loot.TryGetComponent<ItemObject>(out var io))
                {
                    io.itemData = drop.resource.item;
                    io.quantity = 1;
                }

                // Эффект выпадения: Rigidbody
                Rigidbody rb = loot.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = false;
                    rb.useGravity = true;

                    // Сила выброса
                    Vector3 impulse =
                        dropOrigin.forward * Random.Range(1.5f, 3f) +   // вперёд
                        Vector3.up * Random.Range(2f, 4f) +            // подпрыгивание
                        Random.insideUnitSphere * 1.5f;                // лёгкий разлёт

                    rb.AddForce(impulse, ForceMode.Impulse);

                    // Случайный момент вращения
                    rb.AddTorque(Random.insideUnitSphere * 5f, ForceMode.Impulse);
                }
            }
        }
    }
}
