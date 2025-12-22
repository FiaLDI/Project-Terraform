using System.Collections.Generic;
using UnityEngine;
using Features.Items.Data;

public class ItemRegistry : MonoBehaviour
{
    [SerializeField] private Item[] items;

    private Dictionary<string, Item> byId;

    public static ItemRegistry Instance { get; private set; }

    private void Awake()
    {
        Instance = this;

        byId = new Dictionary<string, Item>();
        foreach (var item in items)
            byId[item.id] = item;
    }

    public Item Get(string id)
    {
        return byId.TryGetValue(id, out var item) ? item : null;
    }
}
