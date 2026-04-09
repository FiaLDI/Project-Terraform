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
        questComponent = comp;
        net = controller;

        questComponent.Quests.OnChange += OnQuestChanged;
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

                if (!items.TryGetValue(key, out var item))
                {
                    item = Instantiate(prefab, container);
                    items[key] = item;
                }

                item.Bind(key, value, net);
                break;

            case SyncDictionaryOperation.Remove:

                if (items.TryGetValue(key, out var existing))
                {
                    Destroy(existing.gameObject);
                    items.Remove(key);
                }
                break;
        }
    }
}
