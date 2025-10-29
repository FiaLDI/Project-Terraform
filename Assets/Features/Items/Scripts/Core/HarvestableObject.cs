using UnityEngine;
using System.Collections;

public class HarvestableObject : MonoBehaviour
{
    [Header("Resource Settings")]
    public Item resourceData;
    public int dropCount = 1;
    public float baseHarvestTime = 2f;

    [Header("Tool Requirements")]
    public bool requiresTool = true;
    public string[] allowedToolIds;

    private bool isBeingHarvested = false;

    public void TryHarvest(Tool currentTool)
    {
        if (isBeingHarvested) return;

        if (requiresTool && (currentTool == null || !CanBeHarvestedWith(currentTool)))
        {
            Debug.Log("Нужен подходящий инструмент!");
            return;
        }

        StartCoroutine(HarvestRoutine(currentTool));
    }

    private bool CanBeHarvestedWith(Tool tool)
    {
        foreach (string id in tool.canHarvestIds)
            foreach (string allowed in allowedToolIds)
                if (id == allowed) return true;
        return false;
    }

    private IEnumerator HarvestRoutine(Tool tool)
    {
        isBeingHarvested = true;
        float harvestTime = baseHarvestTime / (tool != null ? tool.baseHarvestSpeed : 1f);
        float elapsed = 0f;

        while (elapsed < harvestTime)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        DropItems();
        Destroy(gameObject);
    }

    private void DropItems()
    {
        for (int i = 0; i < dropCount; i++)
        {
            GameObject dropped = Instantiate(resourceData.worldPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
            ItemObject io = dropped.GetComponent<ItemObject>();
            io.itemData = resourceData;
            io.quantity = 1; // на случай рандомного количества
        }
    }
}