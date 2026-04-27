using UnityEngine;
using System.Collections.Generic;
using FishNet.Object.Synchronizing;

public class QuestDebugListUI : MonoBehaviour
{
    [SerializeField] private Transform container;
    [SerializeField] private QuestDebugItemUI prefab;

    private PlayerQuestComponent questComponent;
    private PlayerQuestNetwork net;

    private readonly Dictionary<string, QuestDebugItemUI> items = new();

    public void Init(PlayerQuestComponent comp, PlayerQuestNetwork controller)
    {
        Unsubscribe();
        ClearRenderedItems();

        questComponent = comp;
        net = controller;

        if (questComponent != null)
        {
            questComponent.Quests.OnChange += OnQuestChanged;
            RestoreExisting();
        }
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    private void Unsubscribe()
    {
        if (questComponent != null)
            questComponent.Quests.OnChange -= OnQuestChanged;
    }

    private void RestoreExisting()
    {
        if (questComponent == null)
            return;

        foreach (var kv in questComponent.Quests)
            BindOrCreate(kv.Key, kv.Value);
    }

    private void ClearRenderedItems()
    {
        foreach (var item in items.Values)
        {
            if (item != null)
                Destroy(item.gameObject);
        }

        items.Clear();
    }

    private void BindOrCreate(string key, QuestNetState value)
    {
        if (!items.TryGetValue(key, out var item))
        {
            item = Instantiate(prefab, container);
            items[key] = item;
        }

        item.Bind(key, value, net);
    }

    private void OnQuestChanged(
        SyncDictionaryOperation op,
        string key,
        QuestNetState value,
        bool asServer)
    {
        switch (op)
        {
            case SyncDictionaryOperation.Add:
            case SyncDictionaryOperation.Set:
                BindOrCreate(key, value);
                break;

            case SyncDictionaryOperation.Remove:
                if (items.TryGetValue(key, out var existing))
                {
                    Destroy(existing.gameObject);
                    items.Remove(key);
                }
                break;

            case SyncDictionaryOperation.Clear:
                ClearRenderedItems();
                break;
        }
    }
}
