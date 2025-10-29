using System.Collections.Generic;
using UnityEngine;

public class ResourceSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public int seed = 12345;
    public Vector2Int chunkSize = new Vector2Int(64, 64);

    [Header("Biome Data")]
    public BiomeSpawnTableSO biomeSpawnTable;

    private List<Vector3> spawnedPositions = new List<Vector3>();

    public void GenerateResources()
    {
        if (biomeSpawnTable == null)
        {
            Debug.LogWarning("BiomeSpawnTable not assigned!");
            return;
        }

        Random.InitState(seed);

        foreach (var nodeEntry in biomeSpawnTable.resourceNodes)
        {
            ResourceNodeSO nodeSO = nodeEntry.resourceNode;
            GameObject prefab = nodeSO.prefab;

            if (prefab == null)
            {
                Debug.LogWarning($"Prefab missing for {nodeSO.name}");
                continue;
            }

            int attempts = 200;
            for (int i = 0; i < attempts; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(0, chunkSize.x),
                    0.5f,
                    Random.Range(0, chunkSize.y)
                );

                float noise = Mathf.PerlinNoise(
                    (pos.x + seed) * 0.05f,
                    (pos.z + seed) * 0.05f
                );

                if (noise > nodeSO.noiseThreshold && IsFarEnough(pos, nodeSO.minDistance))
                {
                    GameObject node = Instantiate(prefab, pos, Quaternion.identity, transform);
                    node.name = $"{nodeSO.nodeName}_Node";
                    spawnedPositions.Add(pos);
                }
            }
        }
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

