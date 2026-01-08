using UnityEngine;
using System.Collections.Generic;
using Features.Items.Data;

[CreateAssetMenu(menuName = "Items/Item Registry")]
public class ItemRegistrySO : ScriptableObject
{
    [SerializeField] private Item[] items;

    private Dictionary<string, Item> byId;

    public static ItemRegistrySO Instance
    {
        get
        {
            if (_instance == null)
            {
                // важно: файл должен лежать в Assets/Resources/Databases/ItemRegistry.asset
                _instance = Resources.Load<ItemRegistrySO>("Databases/ItemRegistry");
                if (_instance != null)
                    _instance.BuildCache();
            }
            return _instance;
        }
    }
    private static ItemRegistrySO _instance;

    private void OnEnable()
    {
        // если Unity сама подгрузила asset — тоже ок
        _instance = this;
        BuildCache();
    }

    private void BuildCache()
    {
        byId = new Dictionary<string, Item>();
        foreach (var item in items)
        {
            if (item == null || string.IsNullOrEmpty(item.id))
                continue;
            byId[item.id] = item;
        }
#if UNITY_EDITOR
        Debug.Log($"[ItemRegistrySO] Loaded {byId.Count} items");
#endif
    }

    public Item Get(string id)
    {
        if (string.IsNullOrEmpty(id) || byId == null)
            return null;

        return byId.TryGetValue(id, out var item) ? item : null;
    }
}
