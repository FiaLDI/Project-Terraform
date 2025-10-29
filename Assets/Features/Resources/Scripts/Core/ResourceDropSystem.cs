using UnityEngine;

public static class ResourceDropSystem
{

    public static void Drop(ResourceDropSO dropTable, Transform dropOrigin)
    {
        if (dropTable == null)
        {
            Debug.LogWarning("Drop table is null.");
            return;
        }

        foreach (var drop in dropTable.drops)
        {
            if (drop == null || drop.resource == null) continue;

            float roll = Random.value;
            if (roll > drop.chance) continue;

            int amount = Random.Range(drop.minAmount, drop.maxAmount + 1);

            Debug.Log($"[Drop] {amount} x {drop.resource.resourceName} at {dropOrigin.position}");

        }
    }
}
