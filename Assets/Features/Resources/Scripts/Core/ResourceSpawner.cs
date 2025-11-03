using System.Collections.Generic;
using UnityEngine;

public class ResourceSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public int seed = 12345;
    public Vector2 areaSize = new Vector2(64, 64);

    [Header("Biome Data")]
    public BiomeSpawnTableSO biomeSpawnTable;

    private readonly List<Vector3> spawnedPositions = new List<Vector3>();

    public void GenerateResources()
    {
        if (biomeSpawnTable == null)
        {
            Debug.LogWarning($"⚠️ {name}: BiomeSpawnTable не назначен!");
            return;
        }

        Random.InitState(seed);

        int totalSpawned = 0;

        foreach (var entry in biomeSpawnTable.resourceNodes)
        {
            if (entry.resourceNode == null || entry.resourceNode.prefab == null)
                continue;

            ResourceNodeSO nodeSO = entry.resourceNode;
            GameObject prefab = nodeSO.prefab;

            for (int i = 0; i < 30; i++)
            {
                if (Random.value > entry.spawnChance)
                    continue;

                Vector3 offset = new Vector3(
                    Random.Range(-areaSize.x / 2f, areaSize.x / 2f),
                    1000f,
                    Random.Range(-areaSize.y / 2f, areaSize.y / 2f)
                );

                Vector3 worldPos = transform.position + offset;

                if (Physics.Raycast(worldPos, Vector3.down, out RaycastHit hit, 2000f))
                {
                    if (IsFarEnough(hit.point, nodeSO.minDistance))
                    {
                        Vector3 finalPos = hit.point + Vector3.up * 0.3f;
                        Instantiate(prefab, finalPos, Quaternion.identity, transform);
                        spawnedPositions.Add(finalPos);
                        totalSpawned++;
                    }
                }
            }
        }

        Debug.Log($"✅ {name}: ресурсы заспавнены по таблице ({biomeSpawnTable.name}), всего {totalSpawned} узлов.");
    }

    private bool IsFarEnough(Vector3 position, float minDistance)
    {
        foreach (var existing in spawnedPositions)
        {
            if (Vector3.Distance(existing, position) < minDistance)
                return false;
        }
        return true;
    }

    [ContextMenu("Generate Now")]
    private void DebugGenerate()
    {
        Clear();
        GenerateResources();
    }

    public void Clear()
    {
        foreach (Transform child in transform)
            DestroyImmediate(child.gameObject);
        spawnedPositions.Clear();
    }
}


