using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Spawn/Registry")]
public sealed class SpawnPrefabRegistry : ScriptableObject
{
    [System.Serializable]
    private struct Entry
    {
        public string id;
        public GameObject prefab;
    }

    [SerializeField] private Entry[] entries;

    private Dictionary<string, GameObject> cache;

    private void OnEnable()
    {
        cache = new Dictionary<string, GameObject>();
        foreach (var e in entries)
            cache[e.id] = e.prefab;
    }

    public GameObject Get(string id)
    {
        return cache.TryGetValue(id, out var p) ? p : null;
    }
}
