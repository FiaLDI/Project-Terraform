using System.Collections.Generic;
using UnityEngine;

public class BiomeSpawnTableSO : ScriptableObject
{
    [System.Serializable]
    public class ResourceNodeEntry
    {
        public ResourceNodeSO resourceNode;
        [Range(0f, 1f)] public float spawnChance = 1f;
    }

    [Header("List of resource nodes for this biome")]
    public List<ResourceNodeEntry> resourceNodes = new List<ResourceNodeEntry>();
}
