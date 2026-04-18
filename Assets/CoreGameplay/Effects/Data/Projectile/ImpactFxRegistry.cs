using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "FX/Impact Registry")]
public class ImpactFxRegistrySO : ScriptableObject
{
    [System.Serializable]
    public struct Entry
    {
        public string id;
        public GameObject prefab;
    }

    [SerializeField] private Entry[] entries;

    private Dictionary<string, GameObject> byId;

    // ===============================
    // SINGLETON (как у ItemRegistry)
    // ===============================

    public static ImpactFxRegistrySO Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<ImpactFxRegistrySO>("Databases/ImpactRegistry");
                if (_instance != null)
                    _instance.BuildCache();
            }
            return _instance;
        }
    }

    private static ImpactFxRegistrySO _instance;

    private void OnEnable()
    {
        _instance = this;
        BuildCache();
    }

    private void BuildCache()
    {
        byId = new Dictionary<string, GameObject>();

        foreach (var e in entries)
        {
            if (string.IsNullOrEmpty(e.id) || e.prefab == null)
                continue;

            byId[e.id] = e.prefab;
        }

#if UNITY_EDITOR
        Debug.Log($"[ImpactFxRegistry] Loaded {byId.Count} FX");
#endif
    }

    // ===============================
    // API
    // ===============================

    public GameObject Get(string id)
    {
        if (string.IsNullOrEmpty(id) || byId == null)
            return null;

        return byId.TryGetValue(id, out var prefab)
            ? prefab
            : null;
    }
}
